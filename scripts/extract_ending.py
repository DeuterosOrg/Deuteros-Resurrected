"""Compile Deuteros Disk2's fixed ending, without an emulator/runtime dependency.

Digital PAL reconstruction; analogue output and original-machine cadence are
separate acceptance work. See docs/original-ending-evidence.md for provenance.
"""
import argparse
from array import array
import base64
import hashlib
import json
from pathlib import Path
import struct
import sys
import tempfile
import wave

DISK_SHA = '99909db1e190be02e049084743af44f00e331be6bf2d97b4831ada5fe4c30b4a'
CONTAINER_SHA = 'ff10894b9960e1b70ba10186d9108b837e3af4fa39eaaf1872eaa9e58b0bbf89'
RATE, SAMPLE_RATE, AUDIO_CLOCK = 50, 44100, 3546895


def read(data, offset, size):
    if offset < 0 or size < 0 or offset + size > len(data):
        raise ValueError(f'Truncated source at {offset:#x}, size {size}')
    return data[offset:offset + size]


def u16(data, offset): return int.from_bytes(read(data, offset, 2), 'big')
def u32(data, offset): return int.from_bytes(read(data, offset, 4), 'big')
def signed(value): return value - 65536 if value & 0x8000 else value


def decode_image(data, offset):
    total, header = u16(data, offset), u16(data, offset + 2)
    planar = header >= 200
    height, words_wide = (header & 255 if planar else header), total // 4
    if total == 0 and header == 0: return 0, 0, b''
    if total % 4 or not 0 < words_wide <= 20 or not 0 < height <= 200:
        raise ValueError('Invalid original image dimensions')
    count, pos, words = total * height, offset + 4, []
    while len(words) < count:
        command = read(data, pos, 1)[0]; pos += 1
        mode, run = command >> 6, command & 63
        if mode == 0:
            run = run or (256 if planar else 65536)
            for _ in range(min(run, count - len(words))):
                words.append(u16(data, pos)); pos += 2
        else:
            if mode == 1:
                value = read(data, pos, 1)[0] * 257; pos += 1
            else:
                if mode == 2:
                    run = run * 256 + read(data, pos, 1)[0]; pos += 1
                value = int.from_bytes(read(data, pos, 2), 'little'); pos += 2
            words.extend([value] * min(run or 65536, count - len(words)))
    width = words_wide * 16
    pixels = bytearray(width * height)
    for y in range(height):
        for x in range(words_wide):
            for plane in range(4):
                source = (plane * height + y) * words_wide + x if planar else y * total + x * 4 + plane
                value = words[source]
                for bit in range(16):
                    pixels[y * width + x * 16 + bit] |= ((value >> (15 - bit)) & 1) << plane
    return width, height, bytes(pixels)


def blit(canvas, width, image, x, y, masked=False):
    iw, ih, pixels = image
    for yy in range(max(0, -y), min(ih, len(canvas) // width - y)):
        for xx in range(max(0, -x), min(iw, width - x)):
            value = pixels[yy * iw + xx]
            if value or not masked: canvas[(y + yy) * width + x + xx] = value


class Music:
    """The original custom15-instrument player, one VBlank per tick."""
    def __init__(self, data):
        self.data = data
        self.order_count = u32(data, 0xf0)
        if not 1 <= self.order_count <= 100: raise ValueError('Invalid music order count')
        self.orders = [u16(data, 0xf4 + i * 2) for i in range(self.order_count)]
        pos = 0x1bc + max(self.orders) + 0x400
        self.samples = []
        for i in range(15):
            start, length, volume, loop, loop_length, _ = struct.unpack('>IHHIHH', read(data, i * 16, 16))
            sample = read(data, pos, length * 2) if start else b''
            if volume > 64: raise ValueError('Invalid instrument volume')
            if start: pos += length * 2
            loop_data = bytes(4) if loop_length == 2 else read(sample, loop - start, loop_length * 2)
            self.samples.append((sample, volume, loop_data))
        self.channels = [dict(period=0, slide=0, volume=0, active=False, pending=0,
                              sample=b'', loop=b'', installed=False, position=0.0) for _ in range(4)]
        self.speed, self.counter, self.row, self.order, self.pointer = 6, 0, 64, 0, 0
        self.order_pointer = 0xf2
        self.filter_enabled = False

    def tick(self):
        for ch in self.channels:
            ch['period'] = (ch['period'] + ch['slide']) & 65535
            if ch['pending'] == 2:
                ch['installed'] = True; ch['pending'] = 0
            elif ch['pending'] == 1:
                ch['active'] = True; ch['pending'] = 2
        self.counter += 1
        if self.counter < self.speed: return
        self.counter = 0; self.row += 1
        if self.row == 65:
            self.order += 1; self.row = 1
            if self.order > self.order_count: self.order, self.order_pointer = 1, 0xf2
            self.order_pointer += 2
            self.pointer = 0x1bc + u16(self.data, self.order_pointer)
        pointer = self.pointer; self.pointer += 16
        for i, ch in enumerate(self.channels):
            period, event = u16(self.data, pointer + i * 4), u16(self.data, pointer + i * 4 + 2)
            instrument, effect, value = event >> 12, event >> 8 & 15, event & 255
            if instrument:
                sample, volume, loop = self.samples[instrument - 1]
                ch.update(period=period, slide=0, volume=volume, active=False, pending=1,
                          sample=sample, loop=loop, installed=False, position=0.0)
            if effect == 1: ch['slide'] = -value
            elif effect == 2: ch['slide'] = value
            elif effect == 3:
                if value > 64: raise ValueError('Invalid effect volume')
                ch['volume'] = value
            elif effect == 4: self.row = 64
            elif effect == 5:
                # Source replaces a0 with the order-table address for subsequent channels.
                if not 1 <= value <= self.order_count: raise ValueError('Invalid order jump')
                self.order = value - 1; self.row = 64; pointer = 0xf4 + (value - 1) * 2
                self.order_pointer = pointer
            elif effect in (6, 7): self.filter_enabled = effect == 6
            elif effect == 8:
                if not value: raise ValueError('Zero music speed')
                self.speed = value
            elif effect != 0: raise ValueError(f'Unknown music effect {effect}')


def music_data(container):
    n = u16(container, 4)
    start = u32(container, 6 + (n + 1) * 4)
    end = u32(container, 6 + (n + 4) * 4)
    return read(container, start, end - start)


def text_image(container, font, pointer):
    canvas = bytearray(320 * 200)
    x = y = 0; foreground = 11; background = 0
    bounds = []
    for _ in range(1024):
        value = read(container, pointer, 1)[0]; pointer += 1
        if value == 0: break
        if value == 0x16:
            x, y = read(container, pointer, 2); pointer += 2
            x *= 8; y = min(y, 48) * 4
        elif value in (0x10, 0x11):
            color = read(container, pointer, 1)[0] & 15; pointer += 1
            if value == 0x10: foreground = color
            else: background = color
        elif value == 7: pass # text-row base changes; no later row movement in these strings
        elif 32 <= value <= 122:
            glyph = read(font, (value - 32) * 8, 8)
            pixels = bytes(foreground if row & (128 >> bit) else background for row in glyph for bit in range(8))
            blit(canvas, 320, (8, 8, pixels), x, y)
            bounds.append((x,y)); x += 8
        else: raise ValueError(f'Unsupported ending text control {value}')
    else: raise ValueError('Unterminated ending text')
    if not bounds: raise ValueError('Empty ending text')
    x0, y0 = min(x for x,y in bounds), min(y for x,y in bounds)
    x1, y1 = max(x for x,y in bounds)+8, max(y for x,y in bounds)+8
    if not (0 <= x0 < x1 <= 320 and 0 <= y0 < y1 <= 200): raise ValueError('Text outside frame')
    pixels = b''.join(canvas[y*320+x0:y*320+x1] for y in range(y0,y1))
    return (x1-x0,y1-y0,pixels), x0,y0


def compile_sequence(container, font):
    if u32(container, 0) != len(container): raise ValueError('Container length mismatch')
    n = u16(container, 4)
    if not 1 <= n <= 7 or len(font) != 728: raise ValueError('Invalid stream count or font')
    pointers = [u32(container, 6+i*4) for i in range(n+6)]
    palette_start, table, bank = pointers[n], pointers[-2], pointers[-1]
    palettes = [[u16(container, palette_start+p*32+i*2) for i in range(16)] for p in range(48)]
    images=[];image_map=[];offsets={}
    if not 0 < bank-table <= 256*4 or (bank-table)%4:raise ValueError('Invalid image table')
    for p in range(table,bank,4):
        offset=bank+u32(container,p)
        if offset not in offsets:
            offsets[offset]=len(images);images.append(decode_image(container,offset))
        image_map.append(offsets[offset])
    music = Music(music_data(container))
    streams = []
    for pointer in pointers[:n]:
        image,x,y,state,wait = struct.unpack('>5H',read(container,pointer,10))
        streams.append(dict(image=image,x=signed(x),y=signed(y),state=state,wait=wait,row=0,pc=pointer+10,ret=0,text=0))
    palette=0; ended=False; frames=[]; snapshot=None; snapshot_y=0; text_cache={}
    for frame in range(10000):
        music.tick()
        for s in streams:
            for guard in range(4096):
                if s['state']==0: break
                if s['state']==3 and s['wait']>0: s['wait']-=1; break
                if s['state']==5 and (music.order-1!=s['wait'] or music.row<=s['row']): break
                p=s['pc']; op=u16(container,p); s['pc']+=2
                if op in (0,16):
                    s['state']=0
                    if op==16: ended=True
                elif op==1: s['image']=u16(container,s['pc']);s['pc']+=2
                elif op==2:
                    s['x']=signed(u16(container,s['pc']));s['y']=signed(u16(container,s['pc']+2));s['pc']+=4
                elif op==3: s['state']=3;s['wait']=u16(container,s['pc']);s['pc']+=2
                elif op==4:
                    palette=u16(container,s['pc']);s['pc']+=2
                    if palette>=48:raise ValueError('Invalid palette')
                elif op==5:
                    s['state']=5;s['wait']=u16(container,s['pc']);s['row']=u16(container,s['pc']+2);s['pc']+=4
                elif op in (7,8):
                    key='x' if op==7 else 'y';s[key]=signed((s[key]+u16(container,s['pc']))&65535);s['pc']+=2
                elif op==9:s['pc']-=u32(container,s['pc'])
                elif op==12:s['ret']=s['pc']+4;s['pc']=u32(container,s['pc'])
                elif op==13:
                    if not s['ret']:raise ValueError('Return without call')
                    s['pc']=s['ret']
                elif op==15:s['text']=u32(container,s['pc']);s['pc']+=4;s['image']=254
                elif op==19:pass
                else:raise ValueError(f'Unknown ending opcode {op} at {p:#x}')
            else:raise ValueError('Unbounded ending command loop')
        layers=[]
        for s in streams:
            if s['state']==0 or s['image']==255:continue
            flag=s['image'];index=flag&255;x=s['x']*16;y=s['y'];masked=bool(flag&0x8000)
            if flag==254:
                if s['text'] not in text_cache:
                    img,tx,ty=text_image(container,font,s['text']);images.append(img)
                    text_cache[s['text']]=(len(images)-1,tx,ty)
                index,x,y=text_cache[s['text']];masked=False
            elif flag==65535:
                if snapshot is None:raise ValueError('Missing background snapshot')
                index,x,y,masked=snapshot,0,snapshot_y,False
            else:
                if index>=len(image_map):raise ValueError('Invalid image index')
                index=image_map[index]
                if not images[index][0]:raise ValueError('Empty image referenced')
                if flag&0xc000==0xc000:masked=False
            layers.append([index,x,y,masked])
            if flag!=65535 and flag&0xc000==0xc000:
                canvas=bytearray(320*200)
                for li,lx,ly,lmask in layers:blit(canvas,320,images[li],lx,ly,lmask)
                snapshot_y=max(0,y);height=min(images[index][1],200-snapshot_y)
                images.append((320,height,bytes(canvas[snapshot_y*320:(snapshot_y+height)*320])))
                snapshot=len(images)-1;s['image']=65535
                layers[-1]=[snapshot,0,snapshot_y,False]
        value=[palette,layers]
        if not frames or frames[-1][1:]!=value:frames.append([frame,*value])
        if ended:break
    else:raise ValueError('Ending did not terminate within10000frames')
    end_frame=frame
    for step in range(1,33):
        colors=[sum(max(0,((v>>shift)&15)-step)<<shift for shift in (8,4,0)) for v in palettes[palette]]
        palettes.append(colors);frames.append([end_frame+step,len(palettes)-1,layers])
    frames.append([end_frame+33,0,[]])
    return dict(rate=RATE,end_frame=end_frame,frame_count=end_frame+34,
                images=[[w,h,base64.b64encode(p).decode('ascii')] for w,h,p in images],palettes=palettes,frames=frames)


def render_music(container, frame_count, fade_start=None):
    music=Music(music_data(container));pcm=array('h')
    for frame in range(frame_count):
        # Register volumes/periods update before row effects; instrument triggers mute immediately.
        volumes=[ch['volume'] for ch in music.channels]
        music.tick()
        fade=256 if fade_start is None else min(256,max(0,256-(frame-fade_start+1)*8))
        for _ in range(SAMPLE_RATE//RATE):
            stereo=[0,0]
            for i,ch in enumerate(music.channels):
                if not ch['active'] or not ch['period'] or not ch['sample']:continue
                sample=ch['sample'];position=ch['position']
                if position>=len(sample):
                    # Once DMA reloads, it repeatedly uses the installed loop.
                    sample=ch['loop'] if ch['installed'] else sample
                    position=(position-len(ch['sample']))%len(sample) if sample else 0
                    ch['sample']=sample
                if sample:
                    value=sample[int(position)];value=value-256 if value>=128 else value
                    stereo[0 if i in (0,3) else 1]+=value*(volumes[i]*fade//256)*2
                    ch['position']=position+AUDIO_CLOCK/(ch['period']*SAMPLE_RATE)
            pcm.extend(stereo)
    if sys.byteorder!='little':pcm.byteswap()
    return pcm.tobytes()


def main():
    parser=argparse.ArgumentParser(description=__doc__)
    parser.add_argument('disk2',type=Path);parser.add_argument('output',type=Path)
    args=parser.parse_args();disk=args.disk2.read_bytes()
    if hashlib.sha256(disk).hexdigest()!=DISK_SHA:raise ValueError('Disk2 SHA256 mismatch')
    container=read(disk,0x6e000,310042)
    if hashlib.sha256(container).hexdigest()!=CONTAINER_SHA:raise ValueError('Ending container SHA256 mismatch')
    sequence=compile_sequence(container,read(disk,0x59c8,728))
    sequence['source_sha256']=CONTAINER_SHA
    # Music stops after32fade ticks; one black update precedes replay/release wait.
    pcm=render_music(container,sequence['end_frame']+33,sequence['end_frame']+1)+bytes(882*4)
    args.output.mkdir(parents=True,exist_ok=True)
    with tempfile.TemporaryDirectory(dir=args.output) as directory:
        temp=Path(directory)
        (temp/'sequence.json').write_text(json.dumps(sequence,separators=(',',':'))+'\n',encoding='utf-8')
        with wave.open(str(temp/'music.wav'),'wb') as sound:
            sound.setparams((2,2,SAMPLE_RATE,0,'NONE','not compressed'));sound.writeframes(pcm)
        for name in ('sequence.json','music.wav'):(temp/name).replace(args.output/name)
    print(f"Compiled {len(sequence['frames'])} frame changes, ending cue {sequence['end_frame']}, {sequence['frame_count']/RATE:.2f}s including fade/black")


if __name__=='__main__':main()

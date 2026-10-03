"""Independent tiny source fixtures catch decoder, cue and DMA sequencing bugs."""
import base64
import hashlib
import json
import os
from pathlib import Path
import struct
import unittest

import extract_ending as ending


def fixture(commands):
    """One stream,48palettes,one zero pattern,two 16x1 images; no external disk."""
    palette, music, table, bank = 0x100, 0x700, 0xd00, 0xd08
    b = bytearray(0xd22)
    struct.pack_into('>IH7I', b, 0, len(b), 1, 0x30, palette, music, 0, 0, table, bank)
    struct.pack_into('>5H', b, 0x30, 255, 0, 0, 3, 1)
    struct.pack_into('>'+'H'*len(commands), b, 0x3a, *commands)
    struct.pack_into('>I', b, music+0xf0, 1)
    struct.pack_into('>II', b, table, 0, 13)
    # A full plane0 row (index1), then a full plane1 row (index2).
    b[bank:bank+13] = struct.pack('>HHB4H',4,1,4,0xffff,0,0,0)
    b[bank+13:bank+26] = struct.pack('>HHB4H',4,1,4,0,0xffff,0,0)
    return bytes(b)


class EndingTests(unittest.TestCase):
    def test_literal_planes_and_both_repeat_byte_orders(self):
        w,h,p = ending.decode_image(struct.pack('>HHB4H',4,1,4,0x8000,0x4000,0x2000,0x1000),0)
        self.assertEqual((16,1,bytes([1,2,4,8]+[0]*12)),(w,h,p))
        for encoded in [bytes([0x44,0xff]), bytes([0xc4,0xff,0xff]), bytes([0x80,4,0xff,0xff])]:
            self.assertEqual(bytes([15]*16),ending.decode_image(struct.pack('>HH',4,1)+encoded,0)[2])
        self.assertEqual(bytes([1]*16),ending.decode_image(struct.pack('>HHB4H',4,0x101,4,0xffff,0,0,0),0)[2])

    def test_masked_zero_and_clipping(self):
        for masked,want in [(False,bytes([1,0])),(True,bytes([1,2]))]:
            canvas=bytearray([2,2]); ending.blit(canvas,2,(2,1,bytes([1,0])),0,0,masked)
            self.assertEqual(want,bytes(canvas))
        canvas=bytearray([2]*4);ending.blit(canvas,2,(2,2,bytes([1,3,4,5])),-1,-1)
        self.assertEqual(bytes([5,2,2,2]),bytes(canvas))

    def test_wait_one_and_strict_music_row_then_end_stops_stream(self):
        seq=ending.compile_sequence(fixture([1,0,3,1,1,1,5,0,1,16]),bytes(728))
        self.assertEqual(11,seq['end_frame']) # row1 at tick5 must not satisfy >1; row2 at11.
        frames={f[0]:f for f in seq['frames']}
        self.assertEqual([],frames[0][2])
        self.assertEqual([0,0,0,False],frames[1][2][0])
        self.assertEqual([1,0,0,False],frames[2][2][0])
        self.assertEqual([],frames[11][2])

    def test_background_snapshot_restores_full_rows_after_position_change(self):
        seq=ending.compile_sequence(fixture([2,1,2,1,0xc000,3,1,2,5,5,3,1,16]),bytes(728))
        layers=[f for f in seq['frames'] if f[0]<=2][-1][2]
        self.assertEqual([0,2,False],layers[0][1:])
        image=seq['images'][layers[0][0]]
        self.assertEqual([320,1],image[:2])
        self.assertEqual(bytes(16)+bytes([1]*16)+bytes(288),base64.b64decode(image[2]))

    def test_text_uses_original_glyph_and_stops_at_first_zero(self):
        b=bytearray(fixture([15,0,0x80,3,1,16]));font=bytearray(728)
        b[0x80:0x89]=bytes([0x16,2,3,0x10,11,65,0,66,0])
        font[(65-32)*8:(65-31)*8]=bytes([0xff]*8)
        seq=ending.compile_sequence(bytes(b),bytes(font));layer=seq['frames'][1][2][0]
        self.assertEqual([16,12,False],layer[1:])
        image=seq['images'][layer[0]]
        self.assertEqual([8,8],image[:2]);self.assertEqual(bytes([11]*64),base64.b64decode(image[2]))

    def test_music_speed_above31_is_a_tick_count(self):
        b=bytearray(fixture([5,0,1,16]));struct.pack_into('>HH',b,0x700+0x1bc,0,0x0840)
        seq=ending.compile_sequence(bytes(b),bytes(728))
        self.assertEqual(69,seq['end_frame']) # first row5, then64ticks; no MOD BPM interpretation.

    def test_delayed_dma_and_silent_sentinel(self):
        b=bytearray(fixture([5,0,1,16]));m=0x700
        # Extend music before images so one-shot sample is present without changing compiler fixture.
        music=bytearray(0x1bc+0x400+4)
        struct.pack_into('>IHHIHH',music,0,100,2,64,999999,2,0)
        struct.pack_into('>I',music,0xf0,1)
        struct.pack_into('>HH',music,0x1bc,3547,0x1000)
        music[-4:]=bytes([64]*4)
        b=bytearray(0x40)+music
        struct.pack_into('>IH7I',b,0,len(b),1,0,0,0x40,0,0,len(b),len(b))
        pcm=ending.render_music(bytes(b),9)
        self.assertEqual(bytes(6*882*4),pcm[:6*882*4])
        self.assertEqual((8192,0),struct.unpack_from('<hh',pcm,6*882*4))
        self.assertEqual(bytes(882*4),pcm[-882*4:])

    def test_order_jump_preserves_original_order_table_pointer_increment(self):
        music=bytearray(0x1bc+0x800)
        struct.pack_into('>I',music,0xf0,2)
        struct.pack_into('>2H',music,0xf4,0,0x400)
        struct.pack_into('>2H',music,0x1bc,0,0x0501)
        struct.pack_into('>2H',music,0x1bc+0x400,0,0x0807)
        player=ending.Music(bytes(music))
        for _ in range(12):player.tick()
        self.assertEqual(7,player.speed) #22676storesF4;2243Aadds2beforefetchingF6.
        self.assertEqual((1,1),(player.order,player.row))

    def test_unknown_opcode_and_truncated_image_rejected(self):
        with self.assertRaises(ValueError):ending.compile_sequence(fixture([99]),bytes(728))
        with self.assertRaises(ValueError):ending.decode_image(b'\0\4\0\1\4\xff',0)

    @unittest.skipUnless(os.environ.get('DEUTEROS_ENDING_DISK2'),'original disk path not supplied')
    def test_original_cues_and_artwork(self):
        disk=Path(os.environ['DEUTEROS_ENDING_DISK2']).read_bytes()
        b=disk[0x6e000:0x6e000+310042]
        seq=ending.compile_sequence(b,disk[0x59c8:0x59c8+728])
        self.assertEqual(3669,seq['end_frame'])
        self.assertEqual(50,seq['rate'])
        image=seq['images'][0]
        self.assertEqual((304,162),tuple(image[:2]))
        expected='3902ca507e636dedaf47128173eb9949c287072a087a003134dfe661902fc8f5'
        self.assertEqual(expected,hashlib.sha256(base64.b64decode(image[2])).hexdigest())
        self.assertEqual(3703,seq['frame_count'])


if __name__=='__main__':unittest.main()

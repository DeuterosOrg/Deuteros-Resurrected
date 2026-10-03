using System;
using System.Collections.Generic;
using System.Linq;
using static Deuteros.Code.Enums;
using Godot;

namespace Deuteros.Code.Objects.Battle
{
    public partial class BattleLogic
    {
        public const int ClipMinX = 80;
        public const int ClipMinY = 36;
        public const int ClipMaxX = 296;
        public const int ClipMaxY = 112;
        public int Player1Ships { get; set; }
        public int Player1Power { get; set; }
        public int Player2Ships { get; set; }
        public int Player2Power { get; set; }
        public bool PlayerFled { get { return P1fleeing; } }
        public bool EnemyFled { get { return P2fleeing; } }
        public bool PTLFired { get { return PTLstate != 0; } }

        private bool canFirePtl;

        private Random r;

        private InterStellarShip playerShip;
        private InterStellarShip enemyShip;

        private int P1Level;
        private int P2Level;

        private PTLState PTLstate;
        private int PTLCounter;

        private List<int> P1DroneCoordsX = new List<int>();
        private List<int> P1DroneCoordsY = new List<int>();
        private List<int> P2DroneCoordsX = new List<int>();
        private List<int> P2DroneCoordsY = new List<int>();

        private int p1Counter;
        private int p2Counter;

        private int P1DestroyedShipCounter1;
        private int P1DestroyedShipExplosion1X;
        private int P1DestroyedShipExplosion1Y;
        private int P1DestroyedShipCounter2;
        private int P1DestroyedShipExplosion2X;
        private int P1DestroyedShipExplosion2Y;

        private int P2DestroyedShipCounter1;
        private int P2DestroyedShipExplosion1X;
        private int P2DestroyedShipExplosion1Y;
        private int P2DestroyedShipCounter2;
        private int P2DestroyedShipExplosion2X;
        private int P2DestroyedShipExplosion2Y;

        private bool P1fleeing = false;
        private bool P2fleeing = false;

        private int enemyFleeLevel;


        //human advancing formation
        //human attack formation
        private int[] P1formation1 = { 7, 1, 3, 5, 7, 1, 3, 5 };
        private int[] P1formation2 = { 0, 1, 2, 3, 4, 5, 6, 7 };

        //enemy initial advancing formation
        //enemy attack formation
        private int[] P2formation1 = { 2, 6, 2, 6, 2, 6, 2, 6 };
        private int[] P2formation2 = { 7, 6, 5, 4, 3, 2, 1, 0 };
        private int droneoffset;

        private int[] PTLcoords = {
            0x00,0x00,0x70,0x44,0x5C,0x3F,0x4C,0x3B,0x3F,0x38,0x33,0x34,0x28,0x31,0x24,0x2F,
            0x22,0x2E,0x21,0x2D,0x20,0x2C,0x20,0x2B,0x21,0x2A,0x22,0x29,0x23,0x28,0x24,0x27,
            0x26,0x26,0x28,0x26,0x2A,0x26,0x2C,0x26,0x2F,0x26,0x32,0x26,0x36,0x26,0x3A,0x26,
            0x3E,0x26,0x42,0x26,0x47,0x26,0x4C,0x26,0x52,0x26,0x59,0x26,0x61,0x26,0x69,0x26,
            0x71,0x26,0x79,0x26 };

        private BattleState BattleState { get; set; }

        private int Player1FleetPosX { get; set; }
        private int Player1FleetPosY { get; set; }
        private int Player2FleetPosX { get; set; }
        private int Player2FleetPosY { get; set; }

        private byte[] battleFactors = { 0x10,0x1E,0x0C,0x0F,0x05,0x12,0x17,0x1E,0x09,0x0B,0x07,0x09,0x0E,0x09,0x0C,0x10,
                0x6,0x08,0x32,0x07,0x0A,0x07,0x09,0x0D,0x05,0x06,0x04,0x05,0x08,0x06,0x07,0x0A,
                0x04,0x05,0x03,0x04,0x06,0x04,0x05,0x07,0x03,0x04,0x02,0x03,0x04,0x03,0x04,0x05,
                0x02,0x03,0x01,0x02,0x03,0x02,0x0C,0x03,0x01,0x02,0x01,0x02,0x02,0x02,0x0C,0x04,
                0x0A,0x28,0x03,0x05,0x1A,0x01,0x14,0x01 };
        private byte[] battleFactors2 = { 0x0A, 0x28, 0x03, 0x05, 0x1A, 0x01, 0x14, 0x01 };

        private byte[] dronelist = {
            0xFF,0xFF,0xFF,0xFF,0xFF,0xFF,0xA6,0xC4,0xB6,0xFF,0xFF,0xFF,0xFF,0xFF,0xFF,0xFF,
            0xAD,0xFF,0xFF,0xAC,0xFF,0xA5,0x7D,0x8A,0x7F,0x8E,0x81,0xFF,0xC3,0xB2,0xFF,0xFF,
            0xFF,0xC0,0xA9,0xFF,0xA3,0x8F,0x60,0x71,0x59,0x73,0x62,0x96,0xFF,0xFF,0xBA,0xFF,
            0xFF,0xB0,0xFF,0xFF,0x91,0x5E,0x47,0x51,0x49,0x53,0x4B,0x64,0x83,0xFF,0xBB,0xB5,
            0xB9,0xFF,0xA1,0x88,0x6F,0x44,0x1F,0x24,0x1D,0x26,0x22,0x4D,0x79,0x98,0xFF,0xFF,
            0xB1,0xC7,0x93,0x6D,0x57,0x3D,0x28,0x2B,0x0A,0x2E,0x29,0x40,0x66,0x77,0xFF,0xFF,
            0xFF,0x9F,0x8B,0x5C,0x42,0x19,0x2F,0x0E,0x06,0x0D,0x32,0x1B,0x4F,0x69,0x9A,0xFF,
            0xA8,0x86,0x6B,0x56,0x39,0x33,0x12,0x13,0x02,0x16,0x0F,0x36,0x3C,0x5B,0x75,0x9B,
            0x9D,0x7B,0x5B,0x41,0x35,0x17,0x07,0x03,0x00,0x04,0x08,0x18,0x38,0x50,0x6A,0x84,
            0xBF,0x85,0x6C,0x55,0x3A,0x34,0x11,0x15,0x01,0x14,0x10,0x37,0x3B,0x5C,0x76,0x9C,
            0xFF,0x9E,0x8C,0x5D,0x43,0x1A,0x30,0x0B,0x05,0x0C,0x31,0x1C,0x4E,0x68,0x99,0xFF,
            0xFF,0xC6,0x94,0x6E,0x58,0x3E,0x27,0x2D,0x09,0x2C,0x2A,0x3F,0x67,0x78,0xFF,0xFF,
            0xAE,0xFF,0xA0,0x87,0x70,0x45,0x20,0x23,0x1E,0x25,0x21,0x4C,0x7A,0x97,0xFF,0xFF,
            0xFF,0xB8,0xFF,0xFF,0x92,0x5F,0x46,0x52,0x48,0x54,0x4A,0x65,0x82,0xBE,0xFF,0xB4,
            0xFF,0xC1,0xAA,0xFF,0xA2,0x90,0x61,0x72,0x5A,0x74,0x63,0x95,0xFF,0xFF,0xBC,0xFF,
            0xAF,0xFF,0xFF,0xAB,0xFF,0xA4,0x7C,0x89,0x7E,0x8D,0x80,0xFF,0xFF,0xB3,0xBD,0xFF,
            0xFF,0xC2,0xFF,0xFF,0xFF,0xFF,0xA7,0xC5,0xB7,0xFF,0xFF,0xFF,0xFF,0xFF,0xFF,0xFF};

        private Label p1power { get; set; }
        private Label p1ships { get; set; }
        private Label p2power { get; set; }
        private Label p2ships { get; set; }

        private Control battleCanvas { get; set; }

        public BattleLogic (InterStellarShip p1, InterStellarShip p2, int p2fleeLevel, Label player1powerlabel, Label player1shipslabel, Label player2powerlabel, Label player2shipslabel, Control battlecanvas)
        {
            p1power = player1powerlabel;
            p1ships = player1shipslabel;
            p2power = player2powerlabel;
            p2ships = player2shipslabel;
            battleCanvas = battlecanvas;

            droneoffset = 0;
            Player1Ships = p1.DroneCount;
            P1Level = (p1.Pilot == null ? 0 : p1.Pilot.GetLevel()) + 4;
            Player1Power = P1Level * Player1Ships;
            Player1FleetPosX = 92;
            Player1FleetPosY = 75;

            Player2Ships = p2.DroneCount;
            P2Level = (p2.Pilot == null ? 0 : p2.Pilot.GetLevel()) + 4;
            Player2Power = P2Level * Player2Ships;
            Player2FleetPosX = 280;
            Player2FleetPosY = 76;

            BattleState = Enums.BattleState.NotStarted;

            playerShip = p1;
            enemyShip = p2;

            r = new Random();

            enemyFleeLevel = p2fleeLevel;
            canFirePtl = p1.PTL && p1.Fuel > 100;

            while (P1DroneCoordsX.Count() < 300) P1DroneCoordsX.Add(0);
            while (P1DroneCoordsY.Count() < 300) P1DroneCoordsY.Add(0);

            while (P2DroneCoordsX.Count() < 300) P2DroneCoordsX.Add(0);
            while (P2DroneCoordsY.Count() < 300) P2DroneCoordsY.Add(0);

            P1DestroyedShipCounter1 = 0;
            P1DestroyedShipCounter2 = 0;
            P2DestroyedShipCounter1 = 0;
            P2DestroyedShipCounter2 = 0;

            PTLstate = 0;

            var n = 0;
            var droney = -32;
            for (int n1 = 0; n1 < 17; n1++)
            {
                var dronex = -32;
                for (var n2 = 0; n2 < 16; n2++)
                {
                    var v = dronelist[n++];
                    if (v != 0xFF)
                    {
                        P1DroneCoordsX[v] = dronex;
                        P1DroneCoordsY[v] = droney;

                        P2DroneCoordsX[v] = -dronex;
                        P2DroneCoordsY[v] = droney;

                    }
                    dronex += 4;
                }
                droney += 3;
            }
        }

        private void DoBattleRound()
        {
            byte[] roundfactors;

            switch (BattleState)
            {
                case BattleState.NotStarted:
                    BattleState = BattleState.FleetsAdvancing;
                    break;

                case BattleState.FleetsAdvancing:
                    if (Player1FleetPosX < 196) Player1FleetPosX++;
                    if (Player2FleetPosX > 196) Player2FleetPosX--;
                    if ((Player2FleetPosX == Player1FleetPosX) || P1fleeing) BattleState = BattleState.FleetsInBattle;
                    break;

                case BattleState.FleetsInBattle:

                    int factor1, factor2;

                    if (P1fleeing || P2fleeing)
                    {
                        if (P1fleeing)
                        {
                            Player1FleetPosX -= 2;
                            if (Player1FleetPosX < 24)
                            {
                                BattleState = BattleState.BattleEnded;
                            }
                        }

                        if (P2fleeing)
                        {
                            Player2FleetPosX += 2;
                            if (Player2FleetPosX == 330)
                            {
                                BattleState = BattleState.BattleEnded;
                            }
                        }

                    }
                    else
                    {

                        if (Player2Power > Player1Power)
                        {
                            factor1 = (Player2Power + 1) / (Player1Power + 1);
                            factor2 = 0;
                            if (factor1 == 1)
                            {
                                factor1 = 0;
                                roundfactors = battleFactors2;
                            }
                            else
                            {
                                roundfactors = battleFactors;
                            }
                        }
                        else
                        {
                            factor1 = 0;
                            factor2 = (Player1Power + 1) / (Player2Power + 1); ;

                            if (factor2 == 1)
                            {
                                factor2 = 0;
                                roundfactors = battleFactors2;
                            }
                            else
                            {
                                roundfactors = battleFactors;
                            }
                        }
                        if (factor1 > 7) factor1 = 7;
                        if (factor2 > 7) factor2 = 7;
                        factor1 = (factor1 * 8) + r.Next(8);
                        factor2 = (factor2 * 8) + r.Next(8);

                        if (p1Counter == 0)
                        {
                            p1Counter = roundfactors[factor1];
                            if (Player2Power > Player1Power)
                            {
                                p1Counter = p1Counter / 2 + 1;
                            }
                        }

                        if (p2Counter == 0)
                        {
                            p2Counter = roundfactors[factor2];
                            if (Player1Power >= Player2Power)
                            {
                                p2Counter = p2Counter / 2 + 1;
                            }
                        }
                    }
                    break;

                case BattleState.BattleEnded:
                    playerShip.DroneCount = Player1Ships;
                    enemyShip.DroneCount = Player2Ships;

                    if (P1fleeing) playerShip.EngageEngine();
                    if (P2fleeing) enemyShip.EngageEngine();

                    break;
            }
        }

        private void ApplyRoundResults()
        {
            if (BattleState == BattleState.FleetsInBattle && !P1fleeing && !P2fleeing)
            {
                if (p1Counter != 0)
                {
                    p1Counter--;
                    if (p1Counter == 0)
                    {
                        if (P1DestroyedShipCounter1 == 0 || P1DestroyedShipCounter2 == 0)
                        {

                            Player1Ships--;
                            if (Player1Ships <= 0) BattleState = BattleState.BattleEnded;
                            Player1Power = P1Level * Player1Ships;
                            if (P1DestroyedShipCounter1 == 0)
                            {
                                P1DestroyedShipCounter1 = 3;
                                P1DestroyedShipExplosion1X = P1DroneCoordsX[Player1Ships] + Player1FleetPosX;
                                P1DestroyedShipExplosion1Y = P1DroneCoordsY[Player1Ships] + Player1FleetPosY;

                            }
                            else
                            {
                                P1DestroyedShipCounter2 = 3;
                                P1DestroyedShipExplosion2X = P1DroneCoordsX[Player1Ships] + Player1FleetPosX;
                                P1DestroyedShipExplosion2Y = P1DroneCoordsY[Player1Ships] + Player1FleetPosY;
                            }
                        }
                    }
                }

                if (p2Counter != 0)
                {
                    p2Counter--;
                    if (p2Counter == 0)
                    {
                        if (P2DestroyedShipCounter1 == 0 || P2DestroyedShipCounter2 == 0)
                        {
                            Player2Ships--;
                            if (Player2Ships <= 0) BattleState = BattleState.BattleEnded;
                            if (Player2Ships <= enemyFleeLevel) P2fleeing = true;
                            Player2Power = P2Level * Player2Ships;
                            if (P2DestroyedShipCounter1 == 0)
                            {
                                P2DestroyedShipCounter1 = 3;
                                P2DestroyedShipExplosion1X = P2DroneCoordsX[Player2Ships] + Player2FleetPosX;
                                P2DestroyedShipExplosion1Y = P2DroneCoordsY[Player2Ships] + Player2FleetPosY;
                            }
                            else
                            {
                                P2DestroyedShipCounter2 = 3;
                                P2DestroyedShipExplosion2X = P2DroneCoordsX[Player2Ships] + Player2FleetPosX;
                                P2DestroyedShipExplosion2Y = P2DroneCoordsY[Player2Ships] + Player2FleetPosY;
                            }
                        }
                    }
                }
            }
        }

        private bool Clip(int x, int y)
        {
            return (x >= ClipMinX) && (x < ClipMaxX) && (y >= ClipMinY) && (y < ClipMaxY);
        }

        private void DrawRect(int x, int y, int wid, int height, Color c)
        {
            x -= ClipMinX;
            y -= ClipMinY;
            battleCanvas.DrawRect(new Rect2(x, y, wid, height), c);
            //g.DrawRectangle(new Pen(c, 1), x, y, wid, height);
        }

        private void DrawHorizLine(int x, int y, int wid, Color c)
        {
            x -= ClipMinX;
            y -= ClipMinY;
            battleCanvas.DrawLine(new Vector2(x, y), new Vector2(x + wid, y), c);
            //g.DrawLine(new Pen(c, 1), x, y, x + wid, y);
        }

        private void DrawPixel(int x, int y, Color c)
        {
            x -= ClipMinX;
            y -= ClipMinY;
            battleCanvas.DrawLine(new Vector2(x, y), new Vector2(x + 1, y), c);
            //g.DrawLine(new Pen(c, 1), x - ClipMinX, y - ClipMinY, x - ClipMinX + 1, y - ClipMinY);
        }


        private void DrawEllipse(int x, int y, int w, int h, Color c, int penWid)
        {
            w /= 2;
            h /= 2;
            penWid /= 2;
            x += w;
            y += h;
            int w1 = w - penWid;
            int w2 = w + penWid;
            int h1 = h - penWid;
            int h2 = h + penWid;

            for (double theta = 0; theta < 2 * Math.PI; theta += 0.01)
            {
                float cx1 = (float)(x + w1 * Math.Cos(theta));
                float cx2 = (float)(x + w2 * Math.Cos(theta));

                float cy1 = (float)(y + h1 * Math.Sin(theta));
                float cy2 = (float)(y + h2 * Math.Sin(theta));

                battleCanvas.DrawLine(new Vector2(cx1 - ClipMinX, cy1 - ClipMinY), new Vector2(cx2 - ClipMinX, cy2 - ClipMinY), c);
            }
        }

        public void UpdateUI()
        {
            p1power.Text = this.Player1Power.ToString();
            p1ships.Text = this.Player1Ships.ToString();

            p2power.Text = this.Player2Power.ToString();
            p2ships.Text = this.Player2Ships.ToString();


            if (PTLstate == PTLState.Firing)
            {

                if (PTLCounter < 34)
                {
                    var ptlx = PTLcoords[PTLCounter * 2] + 64;
                    var ptly = PTLcoords[PTLCounter * 2 + 1] + 43;

                    if (PTLCounter == 1)
                    {
                        //draw 8 line ptl ptl graphic
                        DrawHorizLine(ptlx, ptly, 6, Colors.White);
                        DrawHorizLine(ptlx + 1, ptly + 1, 7, Colors.White);
                        DrawHorizLine(ptlx + 2, ptly + 2, 8, Colors.White);
                        DrawHorizLine(ptlx + 3, ptly + 3, 9, Colors.White);
                        DrawHorizLine(ptlx + 4, ptly + 4, 9, Colors.White);
                        DrawHorizLine(ptlx + 6, ptly + 5, 8, Colors.White);
                        DrawHorizLine(ptlx + 8, ptly + 6, 7, Colors.White);
                        DrawHorizLine(ptlx + 10, ptly + 7, 6, Colors.White);

                    }
                    else if (PTLCounter == 2)
                    {
                        //draw 6 line ptl ptl graphic
                        DrawHorizLine(ptlx, ptly, 4, Colors.White);
                        DrawHorizLine(ptlx + 1, ptly + 1, 5, Colors.White);
                        DrawHorizLine(ptlx + 2, ptly + 2, 6, Colors.White);
                        DrawHorizLine(ptlx + 4, ptly + 3, 6, Colors.White);
                        DrawHorizLine(ptlx + 6, ptly + 4, 5, Colors.White);
                        DrawHorizLine(ptlx + 8, ptly + 5, 7, Colors.White);
                    }
                    else if (PTLCounter < 8)
                    {
                        //draw 3 line ptl ptl graphic
                        DrawHorizLine(ptlx, ptly, 2, Colors.White);
                        DrawHorizLine(ptlx + 2, ptly + 1, 2, Colors.White);
                        DrawHorizLine(ptlx + 4, ptly + 2, 2, Colors.White);
                    }
                    else
                    {
                        //draw single pixel ptl graphic
                        DrawPixel(ptlx, ptly, Colors.White);
                    }
                }
                else if (PTLCounter == 34)
                {
                    //draw explosion 1
                    DrawEllipse(Player1FleetPosX - 60, Player1FleetPosY - 34 + 43 - 20, 120, 36, Colors.Red, 5);
                }
                else if (PTLCounter == 35)
                {
                    //draw explosion 2
                    DrawEllipse(Player1FleetPosX - 96, Player1FleetPosY - 34 + 43 - 32, 192, 60, Colors.Red, 5);
                }
                else if (PTLCounter == 36)
                {
                    //draw explosion 3
                    DrawEllipse(Player1FleetPosX - 60, Player1FleetPosY - 34 + 43 - 20, 120, 40, Colors.Red, 5);
                }
            }


            for (var i = 0; i < Player1Ships; i++)
            {
                if (Clip(P1DroneCoordsX[i] + Player1FleetPosX, P1DroneCoordsY[i] + Player1FleetPosY))
                {
                    DrawPixel(P1DroneCoordsX[i] + Player1FleetPosX, P1DroneCoordsY[i] + Player1FleetPosY - 36 + 43, Colors.Red);
                }
            }

            for (var i = 0; i < Player2Ships; i++)
            {
                if (Clip(P2DroneCoordsX[i] + Player2FleetPosX, P2DroneCoordsY[i] + Player2FleetPosY))
                {
                    DrawPixel(P2DroneCoordsX[i] + Player2FleetPosX, P2DroneCoordsY[i] + Player2FleetPosY - 36 + 43, Colors.Green);
                }
            }

            //draw explosions
            if (P1DestroyedShipCounter1 > 0) drawExplosion(P1DestroyedShipExplosion1X, P1DestroyedShipExplosion1Y, P1DestroyedShipCounter1);
            if (P1DestroyedShipCounter2 > 0) drawExplosion(P1DestroyedShipExplosion2X, P1DestroyedShipExplosion2Y, P1DestroyedShipCounter2);
            if (P2DestroyedShipCounter1 > 0) drawExplosion(P2DestroyedShipExplosion1X, P2DestroyedShipExplosion1Y, P2DestroyedShipCounter1);
            if (P2DestroyedShipCounter2 > 0) drawExplosion(P2DestroyedShipExplosion2X, P2DestroyedShipExplosion2Y, P2DestroyedShipCounter2);

        }

        private void drawExplosion(int explosionX, int explosionY, int frameNumber)
        {
            if (Clip(explosionX, explosionY))
            {
                explosionY = explosionY - 34 + 43;
                if (frameNumber == 3)
                {
                    DrawRect(explosionX - 1, explosionY - 1, 1, 1, Colors.LightGray);
                    DrawRect(explosionX + 2, explosionY - 1, 1, 1, Colors.LightGray);
                    DrawRect(explosionX - 2, explosionY, 4, 1, Colors.LightGray);
                    DrawRect(explosionX + 3, explosionY, 1, 1, Colors.LightGray);
                    DrawRect(explosionX - 1, explosionY + 1, 3, 1, Colors.LightGray);
                    DrawRect(explosionX + 3, explosionY + 1, 1, 1, Colors.LightGray);
                    DrawRect(explosionX - 2, explosionY + 2, 1, 1, Colors.LightGray);
                }

                if (frameNumber == 2)
                {
                    DrawRect(explosionX - 2, explosionY - 1, 1, 1, Colors.DarkGray);
                    DrawRect(explosionX, explosionY - 1, 1, 1, Colors.DarkGray);
                    DrawRect(explosionX + 2, explosionY - 1, 1, 1, Colors.DarkGray);
                    DrawRect(explosionX - 3, explosionY, 1, 1, Colors.DarkGray);
                    DrawRect(explosionX + 3, explosionY, 1, 1, Colors.DarkGray);

                    DrawRect(explosionX - 2, explosionY + 1, 1, 1, Colors.DarkGray);
                    DrawRect(explosionX + 2, explosionY + 1, 2, 1, Colors.DarkGray);
                    DrawRect(explosionX - 1, explosionY + 2, 3, 1, Colors.DarkGray);
                }

                if (frameNumber == 1)
                {
                    DrawRect(explosionX - 1, explosionY - 1, 1, 1, Colors.DarkSlateGray);
                    DrawRect(explosionX + 4, explosionY - 1, 1, 1, Colors.DarkSlateGray);

                    DrawRect(explosionX - 3, explosionY + 1, 1, 1, Colors.DarkSlateGray);
                    DrawRect(explosionX + 3, explosionY + 2, 1, 1, Colors.DarkSlateGray);

                }
            }
            //00100001
            //00000000
            //10000000
            //00000010

            //01010100
            //10000010
            //01000110
            //00111000

            //00100100
            //01111010
            //00111010
            //01000000
        }

        private void MoveShips()
        {
            int[] formationpt2 = { 1, 0, 1, 1, 0, 1, -1, 1, -1, 0, -1, -1, 0, -1, 1, -1 };

            int[] P1formation;
            int[] P2formation;

            if (BattleState == BattleState.FleetsAdvancing)
            {
                P1formation = P1formation1;
                P2formation = P2formation1;
            }
            else
            {
                P1formation = P1formation2;
                P2formation = P2formation2;
            }

            droneoffset = (droneoffset + 1) & 0x7f;
            if (!P1fleeing)
            {
                var P1droneoffset = droneoffset;
                for (var i = 0; i < Player1Ships; i++)
                {
                    P1droneoffset = (P1droneoffset + 1) & 0x7f;


                    var v = P1formation[P1droneoffset / 16] * 2;

                    P1DroneCoordsX[i] = P1DroneCoordsX[i] + formationpt2[v];
                    P1DroneCoordsY[i] = P1DroneCoordsY[i] + formationpt2[v + 1];

                    if (P1DroneCoordsX[i] > 127) P1DroneCoordsX[i] = -127;
                    if (P1DroneCoordsY[i] > 127) P1DroneCoordsY[i] = -127;
                    if (P1DroneCoordsX[i] < -127) P1DroneCoordsX[i] = 127;
                    if (P1DroneCoordsY[i] < -127) P1DroneCoordsY[i] = 127;
                }
            }

            if (!P2fleeing)
            {
                var P2droneoffset = droneoffset;
                for (var i = 0; i < Player2Ships; i++)
                {
                    P2droneoffset = (P2droneoffset + 1) & 0x7f;

                    var v = P2formation[P2droneoffset / 16] * 2;

                    P2DroneCoordsX[i] = P2DroneCoordsX[i] + formationpt2[v];
                    P2DroneCoordsY[i] = P2DroneCoordsY[i] + formationpt2[v + 1];

                    if (P2DroneCoordsX[i] > 127) P2DroneCoordsX[i] = -127;
                    if (P2DroneCoordsY[i] > 127) P2DroneCoordsY[i] = -127;
                    if (P2DroneCoordsX[i] < -127) P2DroneCoordsX[i] = 127;
                    if (P2DroneCoordsY[i] < -127) P2DroneCoordsY[i] = 127;
                }
            }

        }

        private void ProcessPTL()
        {
            if (PTLstate == PTLState.Firing)
            {
                if (PTLCounter == 37)
                {
                    var rnd = r.Next(128) + 2;
                    if (Player2Ships < rnd) Player2Ships = 2; else Player2Ships -= rnd;


                    var rnd2 = r.Next(64);
                    while (rnd2 > rnd) rnd2 = rnd2 / 2;

                    if (Player1Ships < rnd2) Player1Ships = 2; else Player1Ships -= rnd2;

                    p1Counter = 10;
                    p2Counter = 10;

                    PTLstate = PTLState.Fired;
                    PTLCounter = 0;
                }
                else
                {
                    PTLCounter++;
                }
            }
        }
        public void LaunchPTL()
        {
            if (canPTL())
            {
                PTLstate = PTLState.Firing;
                playerShip.Fuel -= 100;
                PTLCounter = 1;
                p1Counter = 100;
                p2Counter = 100;
            }
        }

        public void PlayerFlee()
        {
            if (canFlee()) P1fleeing = true;
        }

        public bool canFlee()
        {
            return RogueCrew.CanCommand(playerShip) && !P1fleeing && !P2fleeing;
        }

        public bool hasPTL()
        {
            return canFirePtl;
        }

        public bool canPTL()
        {
            return RogueCrew.CanCommand(playerShip) && PTLstate == PTLState.NotFired && canFirePtl & BattleState == BattleState.FleetsInBattle && !P1fleeing & !P2fleeing;
        }

        public bool Completed()
        {
            return BattleState == BattleState.BattleEnded;
        }

        public void BattleTick()
        {

            if (BattleState != BattleState.BattleEnded)
            {
                MoveShips();

                DoBattleRound();
                ApplyRoundResults();

                DoBattleRound();
                ApplyRoundResults();


                UpdateUI();
                ProcessPTL();

                if (P1DestroyedShipCounter1 > 0) P1DestroyedShipCounter1--;
                if (P1DestroyedShipCounter2 > 0) P1DestroyedShipCounter2--;
                if (P2DestroyedShipCounter1 > 0) P2DestroyedShipCounter1--;
                if (P2DestroyedShipCounter2 > 0) P2DestroyedShipCounter2--;
            }
        }

    }
}

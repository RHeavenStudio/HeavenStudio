using System;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Bread2Unity
{
    public class BCCAD : DataModel
    {
        public static BCCAD Read(byte[] bytes)
        {
            var bccad = new BCCAD();
            var byteBuffer = new ByteBuffer(bytes);
            byteBuffer.ReadInt(); //timestamp
            bccad.SheetW = byteBuffer.ReadUShort();
            bccad.SheetH = byteBuffer.ReadUShort();
            // Sprites
            var spritesNum = byteBuffer.ReadInt();
            for (var i = 0; i < spritesNum; i++)
            {
                var bccadSprite = new BccadSprite();
                var partsNum = byteBuffer.ReadInt();
                for (var j = 0; j < partsNum; j++)
                {
                    var part = new SpritePart();
                    var region = new Region
                    {
                        RegionX = byteBuffer.ReadUShort(),
                        RegionY = byteBuffer.ReadUShort(),
                        RegionW = byteBuffer.ReadUShort(),
                        RegionH = byteBuffer.ReadUShort()
                    };
                    var result = bccad.Regions.FindIndex(x => Equals(x, region));
                    if (result == -1)
                    {
                        bccad.Regions.Add(region);
                        part.RegionIndex = new RegionIndex(bccad.Regions.Count - 1, 0);
                    }
                    else
                    {
                        var repeatedNumber = bccadSprite.parts.Count(p => p.RegionIndex.Index == result);
                        part.RegionIndex = new RegionIndex(result, repeatedNumber);
                    }

                    part.PosX = byteBuffer.ReadShort();
                    part.PosY = byteBuffer.ReadShort();
                    part.StretchX = byteBuffer.ReadFloat();
                    part.StretchY = byteBuffer.ReadFloat();
                    part.Rotation = byteBuffer.ReadFloat();
                    part.FlipX = byteBuffer.ReadByte() != 0;
                    part.FlipY = byteBuffer.ReadByte() != 0;
                    //Multicolor and screen color
                    part.Multicolor = new Color(Convert.ToInt32(byteBuffer.ReadByte() & 0xff) / 255f,
                        Convert.ToInt32(byteBuffer.ReadByte() & 0xff) / 255f,
                        Convert.ToInt32(byteBuffer.ReadByte() & 0xff) / 255f);
                    part.ScreenColor = new Color(Convert.ToInt32(byteBuffer.ReadByte() & 0xff) / 255f,
                        Convert.ToInt32(byteBuffer.ReadByte() & 0xff) / 255f,
                        Convert.ToInt32(byteBuffer.ReadByte() & 0xff) / 255f);

                    part.Multicolor.a = byteBuffer.ReadByte() / 255f;

                    for (var k = 0; k < 12; k++) byteBuffer.ReadByte();

                    part.designation = byteBuffer.ReadByte();
                    part.unknown = byteBuffer.ReadShort();
                    part.tlDepth = byteBuffer.ReadFloat();
                    part.blDepth = byteBuffer.ReadFloat();
                    part.trDepth = byteBuffer.ReadFloat();
                    part.brDepth = byteBuffer.ReadFloat();
                    bccadSprite.parts.Add(part);
                }

                bccad.Sprites.Add(bccadSprite);
            }

            // Animations
            var animationsNum = byteBuffer.ReadInt();
            for (var i = 0; i < animationsNum; i++)
            {
                var anim = new Animation();
                var nameBuilder = new StringBuilder();
                var length = Convert.ToInt32(byteBuffer.ReadByte());
                for (var j = 0; j < length; j++) nameBuilder.Append(Convert.ToChar(byteBuffer.ReadByte()));

                for (var j = 0; j < 4 - (length + 1) % 4; j++) byteBuffer.ReadByte();

                anim.Name = nameBuilder.ToString();
                anim.InterpolationInt = byteBuffer.ReadInt();
                var stepsNum = byteBuffer.ReadInt();
                for (var j = 0; j < stepsNum; j++)
                {
                    var spriteIndex = byteBuffer.ReadUShort();
                    var step = new AnimationStep
                    {
                        Delay = byteBuffer.ReadUShort(),
                        TranslateX = byteBuffer.ReadShort(),
                        TranslateY = byteBuffer.ReadShort(),
                        Depth = byteBuffer.ReadFloat(),
                        StretchX = byteBuffer.ReadFloat(),
                        StretchY = byteBuffer.ReadFloat(),
                        Rotation = byteBuffer.ReadFloat(),
                        Color = new Color(Convert.ToInt32(byteBuffer.ReadByte() & 0xff) / 255f,
                            Convert.ToInt32(byteBuffer.ReadByte() & 0xff) / 255f,
                            Convert.ToInt32(byteBuffer.ReadByte() & 0xff) / 255f),
                        BccadSprite = bccad.Sprites[spriteIndex]
                    };
                    byteBuffer.ReadByte();
                    byteBuffer.ReadByte();
                    byteBuffer.ReadByte();
                    step.Color.a = Convert.ToInt32(byteBuffer.ReadShort() & 0xFF) / 255f;
                    anim.Steps.Add(step);
                }

                bccad.Animations.Add(anim);
            }

            return bccad;
        }
    }
}
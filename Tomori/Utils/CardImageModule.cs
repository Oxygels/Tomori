﻿using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tesseract;

namespace Tomori.Utils;

internal class CardImageModule
{
    private TesseractEngine _engine = new TesseractEngine("./tessdata", "eng", EngineMode.TesseractAndLstm);
   
    public List<CardData> ReadTextFromCard(byte[] picture, int cardCount)
    {
        using var image = Image.Load(picture);
        var listRes = new List<CardData>();

        for(int i = 0; i < cardCount; i++)
        {
            var croppedCharacter = image.Clone(im => im.Crop(new Rectangle(48 + i * 276, 59 + 0 * 253, 184, 45)).BinaryThreshold(0.21f).GaussianSharpen(0.8f));
            var croppedSeries = image.Clone(im => im.Crop(new Rectangle(48 + i * 276, 59 + 1 * 253, 184, 45)).BinaryThreshold(0.21f).GaussianSharpen(0.8f));

            using var characterBuffer = new MemoryStream(1 << 20);
            using var seriesBuffer = new MemoryStream(1 << 20);

            croppedCharacter.SaveAsPng(characterBuffer);
            croppedSeries.SaveAsPng(seriesBuffer);

            string characterText;
            string seriesText;

            {
                using var pixCharacter = Pix.LoadFromMemory(characterBuffer.ToArray());
                using var characterPage = _engine.Process(pixCharacter);
                characterText = characterPage.GetText().Replace("\n", " ").Replace("  ", " ").Trim();
            }

            {
                using var pixSeries = Pix.LoadFromMemory(seriesBuffer.ToArray());
                using var seriesPage = _engine.Process(pixSeries);
                seriesText = seriesPage.GetText().Replace("\n", " ").Replace("  ", " ").Trim();
            }
           
            listRes.Add(new CardData(characterText, seriesText));

        }
        return listRes;
    }
}

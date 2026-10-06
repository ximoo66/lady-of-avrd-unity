// Authors: Linus Ziesel
// Disclaimer: Written with support of AI tools.
// Created: 2026-07-07 by Linus Ziesel
// Last Modified: 2026-07-07 by Linus Ziesel

﻿using System.IO;
using UnityEngine;

public static class SoundUtils
{
    
 
    public static void NormalizeAudio(float[] samples)
    {
        float max = 0;
        for (int i = 0; i < samples.Length; i++)
        {
            if (Mathf.Abs(samples[i]) > max) max = Mathf.Abs(samples[i]);
        }

        if (max < 0.001f) return; // Silence, don't boost noise

        float scaler = 1.0f / max;
        // Cap the boost to avoid exploding background noise (e.g., max 3x boost)
        scaler = Mathf.Min(scaler, 3.0f); 

        for (int i = 0; i < samples.Length; i++)
        {
            samples[i] *= scaler;
        }
    }       
    
    
     
    public static byte[] EncodeAsWAV(float[] samples, int freq, int channels)
    {
        using (var memoryStream = new MemoryStream(44 + samples.Length * 2))
        {
            using (var writer = new BinaryWriter(memoryStream))
            {
                writer.Write("RIFF".ToCharArray());
                writer.Write(36 + samples.Length * 2);
                writer.Write("WAVE".ToCharArray());
                writer.Write("fmt ".ToCharArray());
                writer.Write(16);
                writer.Write((ushort)1);
                writer.Write((ushort)channels);
                writer.Write(freq);
                writer.Write(freq * channels * 2);
                writer.Write((ushort)(channels * 2));
                writer.Write((ushort)16);
                writer.Write("data".ToCharArray());
                writer.Write(samples.Length * 2);
                foreach (var sample in samples) writer.Write((short)(sample * 32767f));
            }
            return memoryStream.ToArray();
        }
    }


}
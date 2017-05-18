﻿// <copyright file="ResizeWebProcessor.cs" company="James Jackson-South">
// Copyright (c) James Jackson-South and contributors.
// Licensed under the Apache License, Version 2.0.
// </copyright>

namespace ImageSharp.Web.Processors
{
    using System.Collections.Generic;
    using ImageSharp.PixelFormats;
    using ImageSharp.Processing;
    using ImageSharp.Web.Commands;

    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Allows the resizing of images.
    /// </summary>
    public class ResizeWebProcessor : IImageWebProcessor
    {
        private const string Width = "width";
        private const string Height = "height";
        private const string Xy = "rxy";
        private const string Mode = "rmode";
        private const string Sampler = "rsampler";
        private const string Anchor = "ranchor";
        private const string Compand = "compand";

        private static readonly IEnumerable<string> ResizeCommands
            = new[]
            {
                Width,
                Height,
                Xy,
                Mode,
                Sampler,
                Anchor,
                Compand
            };

        /// <inheritdoc/>
        public IEnumerable<string> Commands => ResizeCommands;

        /// <inheritdoc/>
        public Image<TPixel> Process<TPixel>(Image<TPixel> image, HttpContext context, IHostingEnvironment environment, ILogger logger, IQueryCollection commands)
            where TPixel : struct, IPixel<TPixel>
        {
            ResizeOptions options = GetResizeOptions(commands);

            return options == null ? image : image.Resize(options);
        }

        private static ResizeOptions GetResizeOptions(IQueryCollection commands)
        {
            if (!commands.ContainsKey(Width) && !commands.ContainsKey(Height))
            {
                return null;
            }

            CommandParser parser = CommandParser.Instance;
            var options = new ResizeOptions
            {
                Size = ParseSize(commands, parser),
                CenterCoordinates = GetCenter(commands, parser),
                Mode = GetMode(commands, parser),
                Compand = GetCompandMode(commands, parser)
            };

            // Defaults to Bicubic if not set.
            IResampler sampler = GetSampler(commands);
            if (sampler != null)
            {
                options.Sampler = sampler;
            }

            return options;
        }

        private static Size ParseSize(IQueryCollection commands, CommandParser parser)
        {
            int width = parser.ParseValue<int>(commands[Width]);
            int height = parser.ParseValue<int>(commands[Height]);

            return new Size(width, height);
        }

        private static float[] GetCenter(IQueryCollection commands, CommandParser parser)
        {
            return parser.ParseValue<float[]>(commands[Xy]);
        }

        private static ResizeMode GetMode(IQueryCollection commands, CommandParser parser)
        {
            return parser.ParseValue<ResizeMode>(commands[Mode]);
        }

        private static bool GetCompandMode(IQueryCollection commands, CommandParser parser)
        {
            return parser.ParseValue<bool>(commands[Compand]);
        }

        private static IResampler GetSampler(IQueryCollection commands)
        {
            string sampler = commands[Sampler];

            if (sampler != null)
            {
                switch (sampler.ToLowerInvariant())
                {
                    case "nearest": return new NearestNeighborResampler();
                    case "box": return new BoxResampler();
                    case "mitchell": return new MitchellNetravaliResampler();
                    case "catmull": return new CatmullRomResampler();
                    case "lanczos2": return new Lanczos2Resampler();
                    case "lanczos3": return new Lanczos3Resampler();
                    case "lanczos5": return new Lanczos5Resampler();
                    case "lanczos8": return new Lanczos8Resampler();
                    case "welch": return new WelchResampler();
                    case "triangle": return new TriangleResampler();
                    case "hermite": return new HermiteResampler();
                }
            }

            return null;
        }
    }
}
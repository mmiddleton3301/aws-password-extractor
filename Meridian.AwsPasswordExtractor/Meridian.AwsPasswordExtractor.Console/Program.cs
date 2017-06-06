// ----------------------------------------------------------------------------
// <copyright file="Program.cs" company="MTCS (Matt Middleton)">
// Copyright (c) Meridian Technology Consulting Services (Matt Middleton).
// All rights reserved.
// </copyright>
// ----------------------------------------------------------------------------

namespace Meridian.AwsPasswordExtractor.Console
{
    using Meridian.AwsPasswordExtractor.Logic.Definitions;
    using Meridian.AwsPasswordExtractor.Logic.Models;
    using StructureMap;

    /// <summary>
    /// Contains the main entry point for the application,
    /// <see cref="Program.Main(string[])" />. 
    /// </summary>
    public class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        /// <param name="args">
        /// Any arguments passed to the executable upon calling.
        /// </param>
        private static void Main(string[] args)
        {
            // Create our StructureMap registry and...
            Registry registry = new Registry();
            Container container = new Container(registry);

            // Get an instance.
            IExtractorManager extractorManager =
                container.GetInstance<IExtractorManager>();

            // Then extract instance detail.
            InstanceDetail[] instanceDetails =
                extractorManager.ExtractDetails(); 
        }
    }
}
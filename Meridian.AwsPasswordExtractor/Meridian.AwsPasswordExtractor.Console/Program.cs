// ----------------------------------------------------------------------------
// <copyright file="Program.cs" company="MTCS (Matt Middleton)">
// Copyright (c) Meridian Technology Consulting Services (Matt Middleton).
// All rights reserved.
// </copyright>
// ----------------------------------------------------------------------------

namespace Meridian.AwsPasswordExtractor.Console
{
    using CommandLine;
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
            ParserResult<Options> parseResult =
                Parser.Default.ParseArguments<Options>(args);

            parseResult.WithParsed(CommandLineArgumentsParsed);
        }

        /// <summary>
        /// The method triggered when the command line arguments are parsed
        /// with success.
        /// </summary>
        /// <param name="options">
        /// An instance of <see cref="Options" /> containing the parsed command
        /// line arguments.
        /// </param>
        private static void CommandLineArgumentsParsed(Options options)
        {
            // Create our StructureMap registry and...
            Registry registry = new Registry();
            Container container = new Container(registry);

            // Get an instance.
            IExtractorManager extractorManager =
                container.GetInstance<IExtractorManager>();

            // TODO: Allow the pass-thru of AWS access keys.
            // TODO: Include logging/error handling.
            // Then extract instance detail.
            InstanceDetail[] instanceDetails =
                extractorManager.ExtractDetails(
                    options.AwsRegion,
                    options.PasswordEncryptionKeyFile,
                    options.RoleArn);
        }
    }
}
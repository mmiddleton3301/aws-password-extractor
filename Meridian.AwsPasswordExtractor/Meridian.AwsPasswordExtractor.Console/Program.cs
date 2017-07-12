﻿// ----------------------------------------------------------------------------
// <copyright file="Program.cs" company="MTCS (Matt Middleton)">
// Copyright (c) Meridian Technology Consulting Services (Matt Middleton).
// All rights reserved.
// </copyright>
// ----------------------------------------------------------------------------

namespace Meridian.AwsPasswordExtractor.Console
{
    using System;
    using CommandLine;
    using Meridian.AwsPasswordExtractor.Logic.Definitions;
    using NLog;
    using NLog.Config;
    using NLog.Targets;
    using StructureMap;

    /// <summary>
    /// Contains the main entry point for the application,
    /// <see cref="Program.Main(string[])" />. 
    /// </summary>
    public class Program
    {
        /// <summary>
        /// A <see cref="bool" /> value describing the outcome of the main
        /// <see cref="IOutputFileGenerator.CreateOutputFile(Tuple{string, string}, string, System.IO.FileInfo, string, System.IO.FileInfo)" />
        /// method.
        /// </summary> 
        private static bool executionSuccess;

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        /// <param name="args">
        /// Any arguments passed to the executable upon calling.
        /// </param>
        /// <returns>
        /// An exit code. 0 stands for success, anything else is failure.
        /// </returns>
        private static int Main(string[] args)
        {
            int toReturn = 0;

            ParserResult<Options> parseResult =
                Parser.Default.ParseArguments<Options>(args);

            parseResult.WithParsed(CommandLineArgumentsParsed);

            if (!executionSuccess)
            {
                // If exeuction wasn't a success, reflect this in the exit
                // code.
                toReturn = 1;
            }

            return toReturn;
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

            string logLevelStr = options.Verbosity.ToString();
            LogLevel logLevel = LogLevel.FromString(logLevelStr);

            // Get the default NLog configuration - i.e. the one declared
            // in Nlog.config.
            LoggingConfiguration loggingConfiguration =
                LogManager.Configuration;

            // Add a rule for each target, based on the input Verbosity level.
            LoggingRule loggingRule = null;
            foreach (Target target in loggingConfiguration.AllTargets)
            {
                loggingRule = new LoggingRule("*", logLevel, target);

                loggingConfiguration.LoggingRules.Add(loggingRule);
            }
            
            // Get an instance.
            IOutputFileGenerator outputFileGenerator =
                container.GetInstance<IOutputFileGenerator>();

            Tuple<string, string> explicitKey = new Tuple<string, string>(
                options.AccessKeyId,
                options.SecretAccessKey);

            executionSuccess = outputFileGenerator.CreateOutputFile(
                explicitKey,
                options.AwsRegion,
                options.PasswordEncryptionKeyFile,
                options.PasswordEncryptionKeyFileDirectory,
                options.RoleArn,
                options.OutputFile);
        }
    }
}
// ----------------------------------------------------------------------------
// <copyright file="OutputFileGenerator.cs" company="MTCS (Matt Middleton)">
// Copyright (c) Meridian Technology Consulting Services (Matt Middleton).
// All rights reserved.
// </copyright>
// ----------------------------------------------------------------------------

namespace Meridian.AwsPasswordExtractor.Logic
{
    using System.IO;
    using Meridian.AwsPasswordExtractor.Logic.Definitions;
    using Meridian.AwsPasswordExtractor.Logic.Models;

    /// <summary>
    /// Implements <see cref="IOutputFileGenerator" />. 
    /// </summary>
    public class OutputFileGenerator : IOutputFileGenerator
    {
        /// <summary>
        /// An instance of <see cref="IInstanceScanner" />. 
        /// </summary>
        private readonly IInstanceScanner instanceScanner;

        /// <summary>
        /// Initialises a new instance of the
        /// <see cref="OutputFileGenerator" /> class.
        /// </summary>
        /// <param name="instanceScanner">
        /// An instance of <see cref="IInstanceScanner" />. 
        /// </param>
        public OutputFileGenerator(IInstanceScanner instanceScanner)
        {
            this.instanceScanner = instanceScanner;
        }

        /// <summary>
        /// Implements
        /// <see cref="IOutputFileGenerator.CreateOutputFile(string, FileInfo, string, FileInfo)" />. 
        /// </summary>
        /// <param name="region">
        /// The AWS region in which to execute AWS SDK methods against.
        /// </param>
        /// <param name="passwordEncryptionKeyFile">
        /// The location of the password encryption key file.
        /// </param>
        /// <param name="roleArn">
        /// An IAM role ARN to assume prior to pulling back EC2
        /// <see cref="InstanceDetail" />. Optional.
        /// </param>
        /// <param name="outputFile">
        /// The location of the output file.
        /// </param>
        public void CreateOutputFile(
            string region,
            FileInfo passwordEncryptionKeyFile,
            string roleArn,
            FileInfo outputFile)
        {
            // TODO: Allow the pass-thru of AWS access keys.
            // TODO: Include logging/error handling.
            // Then extract instance detail.
            InstanceDetail[] instanceDetails =
                this.instanceScanner.ExtractDetails(
                    region,
                    passwordEncryptionKeyFile,
                    roleArn);

            // Then, save it to the output file. Just text file for now.
            // TODO: Terminals favourite export.
        }
    }
}
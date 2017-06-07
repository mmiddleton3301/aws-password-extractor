// ----------------------------------------------------------------------------
// <copyright file="OutputFileGenerator.cs" company="MTCS (Matt Middleton)">
// Copyright (c) Meridian Technology Consulting Services (Matt Middleton).
// All rights reserved.
// </copyright>
// ----------------------------------------------------------------------------

namespace Meridian.AwsPasswordExtractor.Logic
{
    using System.IO;
    using System.Text;
    using Meridian.AwsPasswordExtractor.Logic.Definitions;
    using Meridian.AwsPasswordExtractor.Logic.Models;

    /// <summary>
    /// Implements <see cref="IOutputFileGenerator" />. 
    /// </summary>
    public class OutputFileGenerator : IOutputFileGenerator
    {
        /// <summary>
        /// An instance of <see cref="IFileSystemProvider" />. 
        /// </summary>
        private readonly IFileSystemProvider fileSystemProvider;

        /// <summary>
        /// An instance of <see cref="IInstanceScanner" />. 
        /// </summary>
        private readonly IInstanceScanner instanceScanner;

        /// <summary>
        /// Initialises a new instance of the
        /// <see cref="OutputFileGenerator" /> class.
        /// </summary>
        /// <param name="fileSystemProvider">
        /// An instance of <see cref="IFileSystemProvider" />. 
        /// </param>
        /// <param name="instanceScanner">
        /// An instance of <see cref="IInstanceScanner" />. 
        /// </param>
        public OutputFileGenerator(
            IFileSystemProvider fileSystemProvider,
            IInstanceScanner instanceScanner)
        {
            this.fileSystemProvider = fileSystemProvider;
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

            if (instanceDetails.Length > 0)
            {
                this.InstanceDetailToTextFile(instanceDetails, outputFile);
            }
        }

        /// <summary>
        /// Writes the contents of an array of <see cref="InstanceDetail" />
        /// instances to a text file (specified by
        /// <paramref name="outputFile" />).
        /// </summary>
        /// <param name="instanceDetails">
        /// An array of <see cref="InstanceDetail" /> instances. 
        /// </param>
        /// <param name="outputFile">
        /// An instance of <see cref="FileInfo" /> describing the location of
        /// the output file.
        /// </param>
        private void InstanceDetailToTextFile(
            InstanceDetail[] instanceDetails,
            FileInfo outputFile)
        {
            // Then, save it to the output file. Just text file for now.
            // TODO: Terminals favourite export.
            StringBuilder stringBuilder = new StringBuilder();

            stringBuilder.AppendLine(
                $"Connection details for {instanceDetails.Length} " +
                $"instance(s) in total.");
            stringBuilder.AppendLine();

            int padding = instanceDetails
                .Length // We want to take the length...
                .ToString()
                .Length; // ... and get the length of the length as a string..

            InstanceDetail instanceDetail = null;
            for (int i = 0; i < instanceDetails.Length; i++)
            {
                instanceDetail = instanceDetails[i];

                stringBuilder.AppendLine(
                    $"{(i + 1).ToString(new string('0', padding))}.\t" +
                    $"{nameof(instanceDetail.Name)}: " +
                    $"{instanceDetail.Name}");
                stringBuilder.AppendLine(
                    $"\t" +
                    $"{nameof(instanceDetail.IPAddress)}: " +
                    $"{instanceDetail.IPAddress}");

                if (!string.IsNullOrEmpty(instanceDetail.Password))
                {
                    stringBuilder.AppendLine(
                        $"\t" +
                        $"{nameof(instanceDetail.Password)}: " +
                        $"{instanceDetail.Password}");
                }

                stringBuilder.AppendLine();
            }

            this.fileSystemProvider.WriteStringToFileInfo(
                outputFile,
                stringBuilder.ToString());
        }
    }
}
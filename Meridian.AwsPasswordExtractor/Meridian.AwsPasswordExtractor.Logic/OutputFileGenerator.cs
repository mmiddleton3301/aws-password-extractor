// ----------------------------------------------------------------------------
// <copyright file="OutputFileGenerator.cs" company="MTCS (Matt Middleton)">
// Copyright (c) Meridian Technology Consulting Services (Matt Middleton).
// All rights reserved.
// </copyright>
// ----------------------------------------------------------------------------

namespace Meridian.AwsPasswordExtractor.Logic
{
    using System;
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
        /// An instance of <see cref="ILoggingProvider" />. 
        /// </summary>
        private readonly ILoggingProvider loggingProvider;

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
        /// <param name="loggingProvider">
        /// An instance of <see cref="ILoggingProvider" />. 
        /// </param>
        public OutputFileGenerator(
            IFileSystemProvider fileSystemProvider,
            IInstanceScanner instanceScanner,
            ILoggingProvider loggingProvider)
        {
            this.fileSystemProvider = fileSystemProvider;
            this.instanceScanner = instanceScanner;
            this.loggingProvider = loggingProvider;
        }

        /// <summary>
        /// Implements
        /// <see cref="IOutputFileGenerator.CreateOutputFile(Tuple{string, string} string, FileInfo, string, FileInfo)" />. 
        /// </summary>
        /// <param name="awsAccessKeys">
        /// An instance of <see cref="Tuple{string, string}" /> containing
        /// firstly the access key id, followed by the the secret access key.
        /// Either value can be null.
        /// </param>
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
            Tuple<string, string> awsAccessKeys,
            string region,
            FileInfo passwordEncryptionKeyFile,
            string roleArn,
            FileInfo outputFile)
        {
            bool argumentsValid = this.ValidateArguments(
                awsAccessKeys,
                region,
                passwordEncryptionKeyFile,
                roleArn,
                outputFile);

            if (argumentsValid)
            {
                // Make awsAccessKeys null if no explicit values were set.
                awsAccessKeys =
                    string.IsNullOrEmpty(awsAccessKeys.Item1)
                        &&
                    string.IsNullOrEmpty(awsAccessKeys.Item2)
                        ?
                    null
                        :
                    awsAccessKeys;

                this.loggingProvider.Debug(
                    $"Pulling back {nameof(InstanceDetail)} for the " +
                    $"specified parameters...");

                // TODO: Test/error scenarios.
                // TODO: Include logging/error handling.
                // Then extract instance detail.
                InstanceDetail[] instanceDetails =
                    this.instanceScanner.ExtractDetails(
                        awsAccessKeys,
                        region,
                        passwordEncryptionKeyFile,
                        roleArn);

                this.loggingProvider.Info(
                    $"Number of {nameof(InstanceDetail)} instances " +
                    $"retrieved - {instanceDetails.Length}.");

                if (instanceDetails.Length > 0)
                {
                    this.loggingProvider.Debug(
                        $"Exporting {nameof(InstanceDetail)} to output file " +
                        $"\"{outputFile.FullName}\"...");

                    this.InstanceDetailToTextFile(instanceDetails, outputFile);

                    this.loggingProvider.Info(
                        $"Export completed with success. Check " +
                        $"\"{outputFile.FullName}\" for instance " +
                        $"information.");
                }
                else
                {
                    this.loggingProvider.Warn(
                        $"No instances could be found (although this may " +
                        $"be expected). No output file will be " +
                        $"composed/written to \"{outputFile.FullName}\".");
                }
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
            this.loggingProvider.Debug(
                $"Composing output file containing {instanceDetails.Length} " +
                $"{nameof(InstanceDetail)} instances before writing to " +
                $"\"{outputFile.FullName}\"...");

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

                this.loggingProvider.Debug(
                    $"Composing detail for \"{instanceDetail}\"...");

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

                this.loggingProvider.Info(
                    $"Finished composing detail for \"{instanceDetail}\".");
            }

            this.loggingProvider.Debug(
                $"Composing of output file content complete. Writing to " +
                $"output file location (\"{outputFile.FullName}\")...");

            this.fileSystemProvider.WriteStringToFileInfo(
                outputFile,
                stringBuilder.ToString());

            this.loggingProvider.Info(
                $"Write to \"{outputFile.FullName}\" complete.");
        }

        /// <summary>
        /// Validates the supplied arguments and returns a <see cref="bool" />
        /// value, indicating whether or not the arguments are valid.
        /// </summary>
        /// <param name="awsAccessKeys">
        /// An instance of <see cref="Tuple{string, string}" /> containing
        /// firstly the access key id, followed by the the secret access key.
        /// Either value can be null.
        /// </param>
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
        /// <returns>
        /// A <see cref="bool" /> value, indicating whether or not he arguments
        /// are valid.
        /// </returns>
        private bool ValidateArguments(
            Tuple<string, string> awsAccessKeys,
            string region,
            FileInfo passwordEncryptionKeyFile,
            string roleArn,
            FileInfo outputFile)
        {
            bool toReturn = true;

            this.loggingProvider.Debug("Validating arguments...");
            
            // awsAccessKeys
            // For this param to be valid, either both values need to be
            // specified OR none at all.
            toReturn =
                (!string.IsNullOrEmpty(awsAccessKeys.Item1)
                    &&
                !string.IsNullOrEmpty(awsAccessKeys.Item2))
                    ||
                (string.IsNullOrEmpty(awsAccessKeys.Item1)
                    &&
                string.IsNullOrEmpty(awsAccessKeys.Item2));

            if (!toReturn)
            {
                this.loggingProvider.Fatal(
                    "If specifying AWS credentials explicitly, you must " +
                    "provide both the Access Key ID and Secret Access Key.");
            }

            // region - validated when injected into logic layer (if the
            //          region isn't valid, an exception will be thrown).
            if (toReturn)
            {
                // passwordEncryptionKeyFile - needs to exist.
                //                             A required parameter.
                toReturn = outputFile.Exists;

                if (!toReturn)
                {
                    this.loggingProvider.Fatal(
                        $"The password encryption file located at: \"" +
                        $"{outputFile.FullName}\" does not exist.");
                }
            }

            this.loggingProvider.Info(
                $"Argument validation result = {toReturn}.");

            // roleArn - optional and also validated on the logic layer.
            return toReturn;
        }
    }
}
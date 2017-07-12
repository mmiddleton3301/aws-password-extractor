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
        /// <see cref="IOutputFileGenerator.CreateOutputFile(Tuple{string, string} string, FileInfo, DirectoryInfo, string, FileInfo)" />. 
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
        /// <param name="passwordEncryptionKeyFileDir">
        /// The location of a directory containing multiple password encryption
        /// key files.
        /// </param>
        /// <param name="roleArn">
        /// An IAM role ARN to assume prior to pulling back EC2
        /// <see cref="InstanceDetail" />. Optional.
        /// </param>
        /// <param name="outputFile">
        /// The location of the output file.
        /// </param>
        /// <returns>
        /// Returns true if the process completed with success, otherwise
        /// false.
        /// </returns>
        public bool CreateOutputFile(
            Tuple<string, string> awsAccessKeys,
            string region,
            FileInfo passwordEncryptionKeyFile,
            DirectoryInfo passwordEncryptionKeyFileDir,
            string roleArn,
            FileInfo outputFile)
        {
            bool toReturn = false;

            try
            {
                this.ExtractDetailsAndWriteInstanceDetailToOutputFile(
                    awsAccessKeys,
                    region,
                    passwordEncryptionKeyFile,
                    passwordEncryptionKeyFileDir,
                    roleArn,
                    outputFile);

                toReturn = true;
            }
            catch (Exception ex)
            {
                // An exception we have either thrown intentionally, or
                // an exception we have not expected.
                // Either way, we want to log this as Fatal.
                this.loggingProvider.Fatal(
                    $"An {nameof(Exception)} was thrown! Detail included.",
                    ex);
            }

            return toReturn;
        }

        /// <summary>
        /// Private entry point for the class, sans any unhandled exception
        /// handling.
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
        /// <param name="passwordEncryptionKeyFileDir">
        /// The location of a directory containing multiple password encryption
        /// key files.
        /// </param>
        /// <param name="roleArn">
        /// An IAM role ARN to assume prior to pulling back EC2
        /// <see cref="InstanceDetail" />. Optional.
        /// </param>
        /// <param name="outputFile">
        /// The location of the output file.
        /// </param>
        private void ExtractDetailsAndWriteInstanceDetailToOutputFile(
            Tuple<string, string> awsAccessKeys,
            string region,
            FileInfo passwordEncryptionKeyFile,
            DirectoryInfo passwordEncryptionKeyFileDir,
            string roleArn,
            FileInfo outputFile)
        {
            this.ValidateArguments(
                awsAccessKeys,
                region,
                passwordEncryptionKeyFile,
                passwordEncryptionKeyFileDir,
                roleArn,
                outputFile);
            
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
                $"Pulling back {nameof(InstanceDetail)} for the specified " +
                $"parameters...");

            // Then extract instance detail.
            InstanceDetail[] instanceDetails =
                this.instanceScanner.ExtractDetails(
                    awsAccessKeys,
                    region,
                    passwordEncryptionKeyFile,
                    passwordEncryptionKeyFileDir,
                    roleArn);

            this.loggingProvider.Info(
                $"Number of {nameof(InstanceDetail)} instances retrieved - " +
                $"{instanceDetails.Length}.");

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
                    $"No instances could be found (although this may be " +
                    $"expected). No output file will be composed/written " +
                    $"to \"{outputFile.FullName}\".");
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
        /// <param name="passwordEncryptionKeyFileDir">
        /// The location of a directory containing multiple password encryption
        /// key files.
        /// </param>
        /// <param name="roleArn">
        /// An IAM role ARN to assume prior to pulling back EC2
        /// <see cref="InstanceDetail" />. Optional.
        /// </param>
        /// <param name="outputFile">
        /// The location of the output file.
        /// </param>
        private void ValidateArguments(
            Tuple<string, string> awsAccessKeys,
            string region,
            FileInfo passwordEncryptionKeyFile,
            DirectoryInfo passwordEncryptionKeyFileDir,
            string roleArn,
            FileInfo outputFile)
        {
            this.loggingProvider.Debug("Validating arguments...");

            // awsAccessKeys
            // For this param to be valid, either both values need to be
            // specified OR none at all.
            bool awsAccessKeysValid =
                (!string.IsNullOrEmpty(awsAccessKeys.Item1)
                    &&
                !string.IsNullOrEmpty(awsAccessKeys.Item2))
                    ||
                (string.IsNullOrEmpty(awsAccessKeys.Item1)
                    &&
                string.IsNullOrEmpty(awsAccessKeys.Item2));

            if (!awsAccessKeysValid)
            {
                throw new InvalidOperationException(
                    "If specifying AWS credentials explicitly, you must " +
                    "provide both the Access Key ID and Secret Access Key.");
            }

            // region - validated when injected into logic layer (if the
            //          region isn't valid, an exception will be thrown).

            // passwordEncryptionKeyFile and passwordEncryptionKeyFileDir -
            // 1) Mandatory that at least one is specified;
            // 2) However, only one can be specified, not both;
            // 3) In the case of passwordEncryptionKeyFile, the file must
            //    exist;
            // 4) In the case of passwordEncryptionKeyFileDir, the directory
            //    must exist and there must be at least one file specified.
            // 1...
            if (passwordEncryptionKeyFile == null && passwordEncryptionKeyFileDir == null)
            {
                throw new InvalidOperationException(
                    "You must specify either a single password encryption " +
                    "key file, or a directory containing multiple password " +
                    "encryption key files.");
            }

            // 2...
            if (passwordEncryptionKeyFile != null && passwordEncryptionKeyFileDir != null)
            {
                throw new InvalidOperationException(
                    "You must specify either a single password encryption " +
                    "key file *OR* a directory containing multiple password " +
                    "encryption key files - you cannot specify both.");
            }

            // 3...
            if (passwordEncryptionKeyFile != null)
            {
                // If we've gotten here, then the single password encryption
                // key file has been specified.
                if (!passwordEncryptionKeyFile.Exists)
                {
                    throw new FileLoadException(
                        $"The password encryption file located at: \"" +
                        $"{passwordEncryptionKeyFile.FullName}\" does " +
                        $"not exist.");
                }
            }
            else
            {
                // Otherwise, it's the directory.
                if (!passwordEncryptionKeyFileDir.Exists)
                {
                    // It does not exist...
                    throw new FileLoadException(
                        $"The password encryption key file directory " +
                        $"located at " +
                        $"{passwordEncryptionKeyFileDir.FullName} does not " +
                        $"exist.");
                }
                else
                {
                    // It does exist. Does it contain files, at least?
                    FileInfo[] dirFiles =
                        passwordEncryptionKeyFileDir.GetFiles();

                    if (dirFiles.Length <= 0)
                    {
                        throw new FileLoadException(
                            $"The password encryption key file directory " +
                            $"located at " +
                            $"{passwordEncryptionKeyFileDir.FullName} does " +
                            $"not contain any files.");
                    }
                }
            }

            // roleArn - optional and also validated on the logic layer.
            this.loggingProvider.Info(
                "Argument validation completed with success.");
        }
    }
}
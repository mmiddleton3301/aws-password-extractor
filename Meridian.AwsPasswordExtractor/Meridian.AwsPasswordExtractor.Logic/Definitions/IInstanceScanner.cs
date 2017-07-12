// ----------------------------------------------------------------------------
// <copyright file="IInstanceScanner.cs" company="MTCS (Matt Middleton)">
// Copyright (c) Meridian Technology Consulting Services (Matt Middleton).
// All rights reserved.
// </copyright>
// ----------------------------------------------------------------------------

namespace Meridian.AwsPasswordExtractor.Logic.Definitions
{
    using System;
    using System.IO;
    using Meridian.AwsPasswordExtractor.Logic.Models;

    /// <summary>
    /// Describes the operations of the extractor manager.
    /// </summary>
    public interface IInstanceScanner
    {
        /// <summary>
        /// Extracts information relating to EC2 instances for the configured
        /// AWS account, and returns the detail.
        /// </summary>
        /// <param name="awsAccessKeys">
        /// An instance of <see cref="Tuple{string, string}" /> containing
        /// firstly the access key id, followed by the the secret access key.
        /// If the variable is null, then credentials based authentication
        /// needs to be used.
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
        /// <returns>
        /// An array of <see cref="InstanceDetail" /> instances. 
        /// </returns>
        InstanceDetail[] ExtractDetails(
            Tuple<string, string> awsAccessKeys,
            string region,
            FileInfo passwordEncryptionKeyFile,
            DirectoryInfo passwordEncryptionKeyFileDir,
            string roleArn);
    }
}
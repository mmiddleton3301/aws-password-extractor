// ----------------------------------------------------------------------------
// <copyright file="IInstanceScanner.cs" company="MTCS (Matt Middleton)">
// Copyright (c) Meridian Technology Consulting Services (Matt Middleton).
// All rights reserved.
// </copyright>
// ----------------------------------------------------------------------------

namespace Meridian.AwsPasswordExtractor.Logic.Definitions
{
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
        /// <returns>
        /// An array of <see cref="InstanceDetail" /> instances. 
        /// </returns>
        InstanceDetail[] ExtractDetails(
            string region,
            FileInfo passwordEncryptionKeyFile,
            string roleArn);
    }
}
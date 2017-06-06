// ----------------------------------------------------------------------------
// <copyright file="IExtractorManager.cs" company="MTCS (Matt Middleton)">
// Copyright (c) Meridian Technology Consulting Services (Matt Middleton).
// All rights reserved.
// </copyright>
// ----------------------------------------------------------------------------

namespace Meridian.AwsPasswordExtractor.Logic.Definitions
{
    using Meridian.AwsPasswordExtractor.Logic.Models;

    /// <summary>
    /// Describes the operations of the extractor manager.
    /// </summary>
    public interface IExtractorManager
    {
        /// <summary>
        /// Extracts information relating to EC2 instances for the configured
        /// AWS account, and returns the detail.
        /// </summary>
        /// <param name="roleArn">
        /// An IAM role ARN to assume prior to pulling back EC2
        /// <see cref="InstanceDetail" />. Optional.
        /// </param>
        /// <returns>
        /// An array of <see cref="InstanceDetail" /> instances. 
        /// </returns>
        InstanceDetail[] ExtractDetails(string roleArn);
    }
}
// ----------------------------------------------------------------------------
// <copyright file="ExtractorManager.cs" company="MTCS (Matt Middleton)">
// Copyright (c) Meridian Technology Consulting Services (Matt Middleton).
// All rights reserved.
// </copyright>
// ----------------------------------------------------------------------------

namespace Meridian.AwsPasswordExtractor.Logic
{
    using System.Collections.Generic;
    using System.Linq;
    using Amazon;
    using Amazon.EC2;
    using Amazon.EC2.Model;
    using Amazon.SecurityToken;
    using Amazon.SecurityToken.Model;
    using Meridian.AwsPasswordExtractor.Logic.Definitions;
    using Meridian.AwsPasswordExtractor.Logic.Models;

    /// <summary>
    /// Implements <see cref="IExtractorManager" />. 
    /// </summary>
    public class ExtractorManager : IExtractorManager
    {
        /// <summary>
        /// The number of instances to return in one go - this value is
        /// basically fed into
        /// <see cref="DescribeInstancesRequest.MaxResults" />. 
        /// </summary>
        private const int DescribeInstancesMaxResults = 10;

        /// <summary>
        /// Initialises a new instance of the <see cref="ExtractorManager" />
        /// class.
        /// </summary>
        public ExtractorManager()
        {
            // Nothing for now...
        }

        /// <summary>
        /// Implements <see cref="IExtractorManager.ExtractDetails()" />. 
        /// </summary>
        /// <param name="region">
        /// The AWS region in which to execute AWS SDK methods against.
        /// </param>
        /// <param name="roleArn">
        /// An IAM role ARN to assume prior to pulling back EC2
        /// <see cref="InstanceDetail" />. Optional.
        /// </param>
        /// <returns>
        /// An array of <see cref="InstanceDetail" /> instances. 
        /// </returns>
        public InstanceDetail[] ExtractDetails(
            string region,
            string roleArn)
        {
            InstanceDetail[] toReturn = null;

            RegionEndpoint regionEndpoint =
                RegionEndpoint.GetBySystemName(region);

            AmazonEC2Config amazonEC2Config = new AmazonEC2Config()
            {
                // TODO: Will need to pass this in. Just want to test it works,
                //       for now.
                RegionEndpoint = regionEndpoint
            };

            IAmazonEC2 amazonEC2 = null;
            if (string.IsNullOrEmpty(roleArn))
            {
                amazonEC2 = new AmazonEC2Client(amazonEC2Config);
            }
            else
            {
                amazonEC2 = this.AssumeRoleAndCreateEC2Client(
                    amazonEC2Config,
                    roleArn);
            }

            // First, list the instances for the configured account.
            DescribeInstancesRequest describeInstancesRequest =
                new DescribeInstancesRequest()
                {
                    MaxResults = DescribeInstancesMaxResults
                };

            List<Reservation> allReservations = new List<Reservation>();
            DescribeInstancesResponse describeInstancesResponse = null;
            do
            {
                describeInstancesResponse =
                    amazonEC2.DescribeInstances(describeInstancesRequest);

                allReservations.AddRange(
                    describeInstancesResponse.Reservations);

                describeInstancesRequest.NextToken =
                    describeInstancesResponse.NextToken;
            }
            while (describeInstancesResponse.NextToken != null);

            return toReturn;
        }

        /// <summary>
        /// Constructs an <see cref="AmazonEC2Client" /> instance with the
        /// temporary details generated from role assumption.
        /// </summary>
        /// <param name="amazonEC2Config">
        /// An instance of <see cref="AmazonEC2Config" />. 
        /// </param>
        /// <param name="roleArn">
        /// An IAM role ARN to assume.
        /// </param>
        /// <returns>
        /// A configured instance of <see cref="AmazonEC2Client" />. 
        /// </returns>
        private AmazonEC2Client AssumeRoleAndCreateEC2Client(
            AmazonEC2Config amazonEC2Config,
            string roleArn)
        {
            AmazonEC2Client toReturn = null;

            AmazonSecurityTokenServiceConfig amazonSecurityTokenServiceConfig =
                new AmazonSecurityTokenServiceConfig()
                {
                    // Nothing for now...
                };

            IAmazonSecurityTokenService amazonSecurityTokenService =
                new AmazonSecurityTokenServiceClient(
                    amazonSecurityTokenServiceConfig);

            // Just use the latter part of the ARN as the session name.
            string roleSessionName = roleArn.Split('/')
                .Last();

            AssumeRoleRequest assumeRoleRequest = new AssumeRoleRequest()
            {
                RoleArn = roleArn,
                RoleSessionName = roleSessionName
            };

            // Get the temporary credentials using the specified role.
            AssumeRoleResponse assumeRoleResponse =
                amazonSecurityTokenService.AssumeRole(assumeRoleRequest);

            Credentials roleCreds = assumeRoleResponse.Credentials;

            toReturn = new AmazonEC2Client(
                roleCreds,
                amazonEC2Config);

            return toReturn;
        }
    }
}

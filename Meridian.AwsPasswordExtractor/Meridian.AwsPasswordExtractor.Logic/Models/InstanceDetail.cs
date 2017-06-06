// ----------------------------------------------------------------------------
// <copyright file="InstanceDetail.cs" company="MTCS (Matt Middleton)">
// Copyright (c) Meridian Technology Consulting Services (Matt Middleton).
// All rights reserved.
// </copyright>
// ----------------------------------------------------------------------------

namespace Meridian.AwsPasswordExtractor.Logic.Models
{
    using System.Linq;
    using System.Net;

    /// <summary>
    /// Contains information regarding a particular instance, in particular
    /// its <see cref="InstanceDetail.IPAddress" /> and
    /// <see cref="InstanceDetail.Password" />.  
    /// </summary>
    public class InstanceDetail
    {
        /// <summary>
        /// Gets or sets the name of the instance.
        /// </summary>
        public string Name
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the IP address of the instance.
        /// </summary>
        public IPAddress IPAddress
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the password for the root/administrator user of
        /// the instance.
        /// </summary>
        public string Password
        {
            get;
            set;
        }

        /// <summary>
        /// Overrides <see cref="object.ToString()" />. 
        /// </summary>
        /// <returns>
        /// A string that represents the current object.
        /// </returns>
        public override string ToString()
        {
            string toReturn =
                $"Instance Detail (" +
                $"Name = {this.Name}, " +
                $"IPAddress = {IPAddress}";

            if (!string.IsNullOrEmpty(this.Password))
            {
                string passMasked =
                    string.Join(
                        string.Empty,
                        this.Password.ToArray().Select(x => "*").ToArray());

                toReturn += $", Password = {passMasked}";
            }

            toReturn += $")";

            return toReturn;
        }
    }
}
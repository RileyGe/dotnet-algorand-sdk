/* 
 * Indexer
 *
 * Algorand ledger analytics API.
 *
 * OpenAPI spec version: 2.0
 * 
 * Generated by: https://github.com/swagger-api/swagger-codegen.git
 */
using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.ComponentModel.DataAnnotations;
using SwaggerDateConverter = Algorand.Client.SwaggerDateConverter;

namespace Algorand.V2.Model
{
    /// <summary>
    /// A simplified version of AssetHolding 
    /// </summary>
    [DataContract]
        public partial class MiniAssetHolding :  IEquatable<MiniAssetHolding>, IValidatableObject
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MiniAssetHolding" /> class.
        /// </summary>
        /// <param name="address">address (required).</param>
        /// <param name="amount">amount (required).</param>
        /// <param name="isFrozen">isFrozen (required).</param>
        public MiniAssetHolding(string address = default(string), ulong? amount = default, bool? isFrozen = default(bool?))
        {
            // to ensure "address" is required (not null)
            if (address == null)
            {
                throw new InvalidDataException("address is a required property for MiniAssetHolding and cannot be null");
            }
            else
            {
                this.Address = address;
            }
            // to ensure "amount" is required (not null)
            if (amount == null)
            {
                throw new InvalidDataException("amount is a required property for MiniAssetHolding and cannot be null");
            }
            else
            {
                this.Amount = amount;
            }
            // to ensure "isFrozen" is required (not null)
            if (isFrozen == null)
            {
                throw new InvalidDataException("isFrozen is a required property for MiniAssetHolding and cannot be null");
            }
            else
            {
                this.IsFrozen = isFrozen;
            }
        }
        
        /// <summary>
        /// Gets or Sets Address
        /// </summary>
        [DataMember(Name="address", EmitDefaultValue=false)]
        public string Address { get; set; }

        /// <summary>
        /// Gets or Sets Amount
        /// </summary>
        [DataMember(Name="amount", EmitDefaultValue=false)]
        public ulong? Amount { get; set; }

        /// <summary>
        /// Gets or Sets IsFrozen
        /// </summary>
        [DataMember(Name="is-frozen", EmitDefaultValue=false)]
        public bool? IsFrozen { get; set; }

        /// <summary>
        /// Returns the string presentation of the object
        /// </summary>
        /// <returns>String presentation of the object</returns>
        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append("class MiniAssetHolding {\n");
            sb.Append("  Address: ").Append(Address).Append("\n");
            sb.Append("  Amount: ").Append(Amount).Append("\n");
            sb.Append("  IsFrozen: ").Append(IsFrozen).Append("\n");
            sb.Append("}\n");
            return sb.ToString();
        }
  
        /// <summary>
        /// Returns the JSON string presentation of the object
        /// </summary>
        /// <returns>JSON string presentation of the object</returns>
        public virtual string ToJson()
        {
            return JsonConvert.SerializeObject(this, Formatting.Indented);
        }

        /// <summary>
        /// Returns true if objects are equal
        /// </summary>
        /// <param name="input">Object to be compared</param>
        /// <returns>Boolean</returns>
        public override bool Equals(object input)
        {
            return this.Equals(input as MiniAssetHolding);
        }

        /// <summary>
        /// Returns true if MiniAssetHolding instances are equal
        /// </summary>
        /// <param name="input">Instance of MiniAssetHolding to be compared</param>
        /// <returns>Boolean</returns>
        public bool Equals(MiniAssetHolding input)
        {
            if (input == null)
                return false;

            return 
                (
                    this.Address == input.Address ||
                    (this.Address != null &&
                    this.Address.Equals(input.Address))
                ) && 
                (
                    this.Amount == input.Amount ||
                    (this.Amount != null &&
                    this.Amount.Equals(input.Amount))
                ) && 
                (
                    this.IsFrozen == input.IsFrozen ||
                    (this.IsFrozen != null &&
                    this.IsFrozen.Equals(input.IsFrozen))
                );
        }

        /// <summary>
        /// Gets the hash code
        /// </summary>
        /// <returns>Hash code</returns>
        public override int GetHashCode()
        {
            unchecked // Overflow is fine, just wrap
            {
                int hashCode = 41;
                if (this.Address != null)
                    hashCode = hashCode * 59 + this.Address.GetHashCode();
                if (this.Amount != null)
                    hashCode = hashCode * 59 + this.Amount.GetHashCode();
                if (this.IsFrozen != null)
                    hashCode = hashCode * 59 + this.IsFrozen.GetHashCode();
                return hashCode;
            }
        }

        /// <summary>
        /// To validate all properties of the instance
        /// </summary>
        /// <param name="validationContext">Validation context</param>
        /// <returns>Validation Result</returns>
        IEnumerable<System.ComponentModel.DataAnnotations.ValidationResult> IValidatableObject.Validate(ValidationContext validationContext)
        {
            yield break;
        }
    }
}

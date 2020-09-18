/* 
 * Algod REST API.
 *
 * API endpoint for algod operations.
 *
 * OpenAPI spec version: 0.0.2
 * Contact: contact@algorand.com
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
using SwaggerDateConverter = Algorand.V2.Algod.Client.SwaggerDateConverter;

namespace Algorand.V2.Algod.Model
{
    /// <summary>
    /// DryrunTxnResult contains any LogicSig or ApplicationCall program debug information and state updates from a dryrun.
    /// </summary>
    [DataContract]
        public partial class DryrunTxnResult :  IEquatable<DryrunTxnResult>, IValidatableObject
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DryrunTxnResult" /> class.
        /// </summary>
        /// <param name="appCallMessages">appCallMessages.</param>
        /// <param name="appCallTrace">appCallTrace.</param>
        /// <param name="disassembly">Disassembled program line by line. (required).</param>
        /// <param name="globalDelta">globalDelta.</param>
        /// <param name="localDeltas">localDeltas.</param>
        /// <param name="logicSigMessages">logicSigMessages.</param>
        /// <param name="logicSigTrace">logicSigTrace.</param>
        public DryrunTxnResult(List<string> appCallMessages = default(List<string>), List<DryrunState> appCallTrace = default(List<DryrunState>), List<string> disassembly = default(List<string>), List globalDelta = default(List), List<AccountStateDelta> localDeltas = default(List<AccountStateDelta>), List<string> logicSigMessages = default(List<string>), List<DryrunState> logicSigTrace = default(List<DryrunState>))
        {
            // to ensure "disassembly" is required (not null)
            if (disassembly == null)
            {
                throw new InvalidDataException("disassembly is a required property for DryrunTxnResult and cannot be null");
            }
            else
            {
                this.Disassembly = disassembly;
            }
            this.AppCallMessages = appCallMessages;
            this.AppCallTrace = appCallTrace;
            this.GlobalDelta = globalDelta;
            this.LocalDeltas = localDeltas;
            this.LogicSigMessages = logicSigMessages;
            this.LogicSigTrace = logicSigTrace;
        }
        
        /// <summary>
        /// Gets or Sets AppCallMessages
        /// </summary>
        [DataMember(Name="app-call-messages", EmitDefaultValue=false)]
        public List<string> AppCallMessages { get; set; }

        /// <summary>
        /// Gets or Sets AppCallTrace
        /// </summary>
        [DataMember(Name="app-call-trace", EmitDefaultValue=false)]
        public List<DryrunState> AppCallTrace { get; set; }

        /// <summary>
        /// Disassembled program line by line.
        /// </summary>
        /// <value>Disassembled program line by line.</value>
        [DataMember(Name="disassembly", EmitDefaultValue=false)]
        public List<string> Disassembly { get; set; }

        /// <summary>
        /// Gets or Sets GlobalDelta
        /// </summary>
        [DataMember(Name="global-delta", EmitDefaultValue=false)]
        public List GlobalDelta { get; set; }

        /// <summary>
        /// Gets or Sets LocalDeltas
        /// </summary>
        [DataMember(Name="local-deltas", EmitDefaultValue=false)]
        public List<AccountStateDelta> LocalDeltas { get; set; }

        /// <summary>
        /// Gets or Sets LogicSigMessages
        /// </summary>
        [DataMember(Name="logic-sig-messages", EmitDefaultValue=false)]
        public List<string> LogicSigMessages { get; set; }

        /// <summary>
        /// Gets or Sets LogicSigTrace
        /// </summary>
        [DataMember(Name="logic-sig-trace", EmitDefaultValue=false)]
        public List<DryrunState> LogicSigTrace { get; set; }

        /// <summary>
        /// Returns the string presentation of the object
        /// </summary>
        /// <returns>String presentation of the object</returns>
        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append("class DryrunTxnResult {\n");
            sb.Append("  AppCallMessages: ").Append(AppCallMessages).Append("\n");
            sb.Append("  AppCallTrace: ").Append(AppCallTrace).Append("\n");
            sb.Append("  Disassembly: ").Append(Disassembly).Append("\n");
            sb.Append("  GlobalDelta: ").Append(GlobalDelta).Append("\n");
            sb.Append("  LocalDeltas: ").Append(LocalDeltas).Append("\n");
            sb.Append("  LogicSigMessages: ").Append(LogicSigMessages).Append("\n");
            sb.Append("  LogicSigTrace: ").Append(LogicSigTrace).Append("\n");
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
            return this.Equals(input as DryrunTxnResult);
        }

        /// <summary>
        /// Returns true if DryrunTxnResult instances are equal
        /// </summary>
        /// <param name="input">Instance of DryrunTxnResult to be compared</param>
        /// <returns>Boolean</returns>
        public bool Equals(DryrunTxnResult input)
        {
            if (input == null)
                return false;

            return 
                (
                    this.AppCallMessages == input.AppCallMessages ||
                    this.AppCallMessages != null &&
                    input.AppCallMessages != null &&
                    this.AppCallMessages.SequenceEqual(input.AppCallMessages)
                ) && 
                (
                    this.AppCallTrace == input.AppCallTrace ||
                    this.AppCallTrace != null &&
                    input.AppCallTrace != null &&
                    this.AppCallTrace.SequenceEqual(input.AppCallTrace)
                ) && 
                (
                    this.Disassembly == input.Disassembly ||
                    this.Disassembly != null &&
                    input.Disassembly != null &&
                    this.Disassembly.SequenceEqual(input.Disassembly)
                ) && 
                (
                    this.GlobalDelta == input.GlobalDelta ||
                    (this.GlobalDelta != null &&
                    this.GlobalDelta.Equals(input.GlobalDelta))
                ) && 
                (
                    this.LocalDeltas == input.LocalDeltas ||
                    this.LocalDeltas != null &&
                    input.LocalDeltas != null &&
                    this.LocalDeltas.SequenceEqual(input.LocalDeltas)
                ) && 
                (
                    this.LogicSigMessages == input.LogicSigMessages ||
                    this.LogicSigMessages != null &&
                    input.LogicSigMessages != null &&
                    this.LogicSigMessages.SequenceEqual(input.LogicSigMessages)
                ) && 
                (
                    this.LogicSigTrace == input.LogicSigTrace ||
                    this.LogicSigTrace != null &&
                    input.LogicSigTrace != null &&
                    this.LogicSigTrace.SequenceEqual(input.LogicSigTrace)
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
                if (this.AppCallMessages != null)
                    hashCode = hashCode * 59 + this.AppCallMessages.GetHashCode();
                if (this.AppCallTrace != null)
                    hashCode = hashCode * 59 + this.AppCallTrace.GetHashCode();
                if (this.Disassembly != null)
                    hashCode = hashCode * 59 + this.Disassembly.GetHashCode();
                if (this.GlobalDelta != null)
                    hashCode = hashCode * 59 + this.GlobalDelta.GetHashCode();
                if (this.LocalDeltas != null)
                    hashCode = hashCode * 59 + this.LocalDeltas.GetHashCode();
                if (this.LogicSigMessages != null)
                    hashCode = hashCode * 59 + this.LogicSigMessages.GetHashCode();
                if (this.LogicSigTrace != null)
                    hashCode = hashCode * 59 + this.LogicSigTrace.GetHashCode();
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
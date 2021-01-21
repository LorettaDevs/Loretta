using System;
using GParse;

namespace Loretta.CodeAnalysis
{
    /// <summary>
    /// A <see cref="Diagnostic"/> template.
    /// </summary>
    public sealed class DiagnosticTemplate
    {
        /// <summary>
        /// Creates a new <see cref="DiagnosticSeverity.Hidden"/> diagnostic template.
        /// </summary>
        /// <param name="id">The diagnostic id.</param>
        /// <param name="descriptionFormat">The diagnostic description format.</param>
        /// <returns></returns>
        public static DiagnosticTemplate Hidden ( String id, String descriptionFormat ) =>
            new ( id, DiagnosticSeverity.Hidden, descriptionFormat );

        /// <summary>
        /// Creates a new <see cref="DiagnosticSeverity.Info"/> diagnostic template.
        /// </summary>
        /// <param name="id">The diagnostic id.</param>
        /// <param name="descriptionFormat">The diagnostic description format.</param>
        /// <returns></returns>
        public static DiagnosticTemplate Info ( String id, String descriptionFormat ) =>
            new ( id, DiagnosticSeverity.Info, descriptionFormat );

        /// <summary>
        /// Creates a new <see cref="DiagnosticSeverity.Warning"/> diagnostic template.
        /// </summary>
        /// <param name="id">The diagnostic id.</param>
        /// <param name="descriptionFormat">The diagnostic description format.</param>
        /// <returns></returns>
        public static DiagnosticTemplate Warning ( String id, String descriptionFormat ) =>
            new ( id, DiagnosticSeverity.Warning, descriptionFormat );

        /// <summary>
        /// Creates a new <see cref="DiagnosticSeverity.Error"/> diagnostic template.
        /// </summary>
        /// <param name="id">The diagnostic id.</param>
        /// <param name="descriptionFormat">The diagnostic description format.</param>
        /// <returns></returns>
        public static DiagnosticTemplate Error ( String id, String descriptionFormat ) =>
            new ( id, DiagnosticSeverity.Error, descriptionFormat );

        /// <summary>
        /// The <see cref="Diagnostic.Id"/>.
        /// </summary>
        public String Id { get; }

        /// <summary>
        /// The default <see cref="Diagnostic.Severity"/>.
        /// </summary>
        public DiagnosticSeverity DefaultSeverity { get; }

        /// <summary>
        /// The <see cref="Diagnostic.Description"/>
        /// </summary>
        public String DescriptionFormat { get; }

        /// <summary>
        /// Initializes a new diagnostic template.
        /// </summary>
        /// <param name="id"><inheritdoc cref="Id" path="/summry"/></param>
        /// <param name="defaultSeverity"><inheritdoc cref="DefaultSeverity" path="/summary"/></param>
        /// <param name="descriptionFormat"><inheritdoc cref="DescriptionFormat" path="/summary"/></param>
        public DiagnosticTemplate ( String id, DiagnosticSeverity defaultSeverity, String descriptionFormat )
        {
            this.Id = id;
            this.DefaultSeverity = defaultSeverity;
            this.DescriptionFormat = descriptionFormat;
        }

        /// <summary>
        /// Synthesizes a new diagnostic from this template.
        /// </summary>
        /// <param name="range">The <see cref="Diagnostic.Range"/>.</param>
        /// <param name="args">The format arguments for the <see cref="DescriptionFormat"/>.</param>
        /// <returns>A new <see cref="Diagnostic"/>.</returns>
        public Diagnostic Synthesize ( SourceRange range, params Object[] args ) =>
            this.Synthesize ( this.DefaultSeverity, range, args );

        /// <summary>
        /// Synthesizes a new diagnostic from this template.
        /// </summary>
        /// <param name="severity">The <see cref="Diagnostic.Severity"/>.</param>
        /// <param name="range">The <see cref="Diagnostic.Range"/>.</param>
        /// <param name="args">The format arguments for the <see cref="DescriptionFormat"/>.</param>
        /// <returns>A new <see cref="Diagnostic"/>.</returns>
        public Diagnostic Synthesize ( DiagnosticSeverity severity, SourceRange range, params Object[] args ) =>
            new ( severity, this.Id, String.Format ( this.DescriptionFormat, args ), range );

        /// <summary>
        /// Reports a new <see cref="Diagnostic"/> to the provided <see cref="DiagnosticList"/>.
        /// </summary>
        /// <param name="diagnostics">The diagnostic list to report the new diagnostic to.</param>
        /// <param name="range"><inheritdoc cref="Synthesize(SourceRange, Object[])"/></param>
        /// <param name="args"><inheritdoc cref="Synthesize(SourceRange, Object[])"/></param>
        public void ReportTo ( DiagnosticList diagnostics, SourceRange range, params Object[] args ) =>
            diagnostics.Report ( this.Synthesize ( range, args ) );

        /// <summary>
        /// Reports a new <see cref="Diagnostic"/> to the provided <see cref="DiagnosticList"/>.
        /// </summary>
        /// <param name="diagnostics">The diagnostic list to report the new diagnostic to.</param>
        /// <param name="severity"><inheritdoc cref="Synthesize(DiagnosticSeverity, SourceRange, Object[])"/></param>
        /// <param name="range"><inheritdoc cref="Synthesize(DiagnosticSeverity, SourceRange, Object[])"/></param>
        /// <param name="args"><inheritdoc cref="Synthesize(DiagnosticSeverity, SourceRange, Object[])"/></param>
        public void ReportTo ( DiagnosticList diagnostics, DiagnosticSeverity severity, SourceRange range, params Object[] args ) =>
            diagnostics.Report ( this.Synthesize ( severity, range, args ) );
    }
}

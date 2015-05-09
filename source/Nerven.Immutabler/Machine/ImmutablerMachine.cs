using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.MSBuild;
using Nerven.Assertion;

namespace Nerven.Immutabler.Machine
{
    [PublicAPI]
    public sealed class ImmutablerMachine
    {
        private readonly string _GeneratedImmutablerFileSufix;
        private readonly ImmutablerTypeDefinitionGenerator _TypeDefinitionGenerator;
        private readonly ImmutablerSyntaxBuilder _SyntaxBuilder;

        public ImmutablerMachine(
            string generatedImmutablerFileSufix = null,
            ImmutablerTypeDefinitionGenerator typeDefinitionGenerator = null,
            ImmutablerSyntaxBuilder syntaxBuilder = null)
        {
            _GeneratedImmutablerFileSufix = generatedImmutablerFileSufix ?? ".Immutabler.Generated";
            _TypeDefinitionGenerator = typeDefinitionGenerator ?? new ImmutablerTypeDefinitionGenerator();
            _SyntaxBuilder = syntaxBuilder ?? new ImmutablerSyntaxBuilder();
        }

        public async Task<Project> PatchProjectAsync(Project project, CancellationToken cancellationToken = default(CancellationToken))
        {
            var _modified = false;
            var _project = project;

            foreach (var _documentId in project.DocumentIds)
            {
                var _document = _project.GetDocument(_documentId);

                if (_document.SupportsSyntaxTree)
                {
                    var _syntaxTree = await _document.GetSyntaxRootAsync(cancellationToken);
                    CompilationUnitSyntax _compilationUnit;
                    if (_syntaxTree?.Language == LanguageNames.CSharp && (_compilationUnit = _syntaxTree as CompilationUnitSyntax) != null)
                    {
                        var _typeDefinition = _TypeDefinitionGenerator.GenerateTypeDefinition(_compilationUnit);
                        if (_typeDefinition != null)
                        {
                            Must.Assert(!(Path.GetFileNameWithoutExtension(_document.Name) ?? string.Empty).EndsWith(_GeneratedImmutablerFileSufix, StringComparison.OrdinalIgnoreCase));

                            var _generatedImmutablerSyntax = _SyntaxBuilder.BuildSyntaxFromTypeDefinition(_typeDefinition);
                            var _generatedImmutablerDocumentName = Path.GetFileNameWithoutExtension(_document.Name) + _GeneratedImmutablerFileSufix + Path.GetExtension(_document.Name);
                            var _generatedImmutablerDocument = _project.Documents.SingleOrDefault(_d => _d.Name.Equals(_generatedImmutablerDocumentName, StringComparison.OrdinalIgnoreCase));
                            Must.Assert(_generatedImmutablerDocument == null || _generatedImmutablerDocument.Name.Equals(_generatedImmutablerDocumentName, StringComparison.Ordinal));
                            if (_generatedImmutablerDocument == null)
                            {
                                _generatedImmutablerDocument = _project.AddDocument(
                                    _generatedImmutablerDocumentName,
                                    _generatedImmutablerSyntax,
                                    folders: _document.Folders);
                                _project = _generatedImmutablerDocument.Project;
                                _modified = true;
                            }
                            else
                            {
                                var _existingGeneratedImmutablerSyntax = await _generatedImmutablerDocument.GetSyntaxRootAsync(cancellationToken);
                                if (!_generatedImmutablerSyntax.IsEquivalentTo(_existingGeneratedImmutablerSyntax))
                                {
                                    _generatedImmutablerDocument = _generatedImmutablerDocument.WithSyntaxRoot(_generatedImmutablerSyntax);
                                    _project = _generatedImmutablerDocument.Project;
                                    _modified = true;
                                }
                            }
                        }
                    }
                }
            }

            return _modified ? _project : null;
        }

        public async Task<Solution> PatchSolutionAsync(Solution solution, CancellationToken cancellationToken = default(CancellationToken))
        {
            var _modified = false;
            var _solution = solution;
            foreach (var _projectId in solution.ProjectIds)
            {
                var _project = _solution.GetProject(_projectId);
                var _newProject = await PatchProjectAsync(_project, cancellationToken);

                if (_newProject != null)
                {
                    _solution = _newProject.Solution;
                    _modified = true;
                }
            }

            return _modified ? _solution : null;
        }

        public async Task<bool> PatchMsbuildProjectAsync(string projectFilePath, CancellationToken cancellationToken = default(CancellationToken))
        {
            using (var _msbuild = MSBuildWorkspace.Create())
            {
                var _project = await _msbuild.OpenProjectAsync(projectFilePath, cancellationToken);
                _project = await PatchProjectAsync(_project, cancellationToken);

                if (_project != null)
                {
                    return _msbuild.TryApplyChanges(_project.Solution);
                }
            }

            return false;
        }

        public async Task<bool> PatchMsbuildSolutionAsync(string solutionFilePath, CancellationToken cancellationToken = default(CancellationToken))
        {
            using (var _msbuild = MSBuildWorkspace.Create())
            {
                var _solution = await _msbuild.OpenSolutionAsync(solutionFilePath, cancellationToken);
                _solution = await PatchSolutionAsync(_solution, cancellationToken);

                if (_solution != null)
                {
                    return _msbuild.TryApplyChanges(_solution);
                }
            }

            return false;
        }
    }
}

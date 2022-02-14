using Loretta.CodeAnalysis.Lua.StatisticsCollector.Mathematics;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using Tsu.Numerics;

namespace Loretta.CodeAnalysis.Lua.StatisticsCollector
{
    internal static class ReportWriter
    {
        private static ICell GetOrCreateCell(this IRow row, int cellIdx) =>
            row.GetCell(cellIdx) ?? row.CreateCell(cellIdx);

        public static void WriteReport(string path, GlobalStatistics globalStatistics, ImmutableArray<FileStatistics> fileStatistics)
        {
            var workbook = new XSSFWorkbook();
            WriteGlobalStatistics(workbook, globalStatistics, fileStatistics.Length);

            fileStatistics = fileStatistics.Sort((a, b) => a.FileName.CompareTo(b.FileName));
            WriteBasicFileStatistics(workbook, fileStatistics);
            WriteTokenFileStatistics(workbook, fileStatistics);
            WriteFeatureStatistics(workbook, fileStatistics);

            using var stream = File.Create(path);
            workbook.Write(stream);
        }

        private static void WriteCells(IRow row, int columnOffset, params object[] cells)
        {
            for (var idx = 0; idx < cells.Length; idx++)
                row.GetOrCreateCell(columnOffset + idx).SetCellValue(cells[idx].ToString());
        }

        private static void WriteStatisticsHeader(IRow row, int columnOffset, string? prefix = null)
        {
            WriteCells(
                row,
                columnOffset,
                $"{prefix} Min",
                $"{prefix} Min (Pretty)",
                $"{prefix} Lower Fence",
                $"{prefix} Lower Fence (Pretty)",
                $"{prefix} Q1",
                $"{prefix} Q1 (Pretty)",
                $"{prefix} Median",
                $"{prefix} Median (Pretty)",
                $"{prefix} Mean",
                $"{prefix} Mean (Pretty)",
                $"{prefix} Q3",
                $"{prefix} Q3 (Pretty)",
                $"{prefix} Upper Fence",
                $"{prefix} Upper Fence (Pretty)",
                $"{prefix} Max",
                $"{prefix} Max (Pretty)",
                $"{prefix} P0",
                $"{prefix} P0 (Pretty)",
                $"{prefix} P25",
                $"{prefix} P25 (Pretty)",
                $"{prefix} P50",
                $"{prefix} P50 (Pretty)",
                $"{prefix} P67",
                $"{prefix} P67 (Pretty)",
                $"{prefix} P80",
                $"{prefix} P80 (Pretty)",
                $"{prefix} P85",
                $"{prefix} P85 (Pretty)",
                $"{prefix} P90",
                $"{prefix} P90 (Pretty)",
                $"{prefix} P95",
                $"{prefix} P95 (Pretty)",
                $"{prefix} P99",
                $"{prefix} P99 (Pretty)",
                $"{prefix} P100",
                $"{prefix} P100 (Pretty)");
        }

        private static void WriteStatistics(IRow row, Statistics statistics, int columnOffset, Func<double, string>? converter = null)
        {
            converter ??= static n => n.ToString();

            WriteCells(
                row,
                columnOffset,
                statistics.Min,
                converter(statistics.Min),
                statistics.LowerFence,
                converter(statistics.LowerFence),
                statistics.Q1,
                converter(statistics.Q1),
                statistics.Median,
                converter(statistics.Median),
                statistics.Mean,
                converter(statistics.Mean),
                statistics.Q3,
                converter(statistics.Q3),
                statistics.UpperFence,
                converter(statistics.UpperFence),
                statistics.Max,
                converter(statistics.Max),
                statistics.P0,
                converter(statistics.P0),
                statistics.P25,
                converter(statistics.P25),
                statistics.P50,
                converter(statistics.P50),
                statistics.P67,
                converter(statistics.P67),
                statistics.P80,
                converter(statistics.P80),
                statistics.P85,
                converter(statistics.P85),
                statistics.P90,
                converter(statistics.P90),
                statistics.P95,
                converter(statistics.P95),
                statistics.P99,
                converter(statistics.P99),
                statistics.P100,
                converter(statistics.P100));
        }

        private static void WriteGlobalStatistics(XSSFWorkbook workbook, GlobalStatistics globalStatistics, int totalFiles)
        {
            var sheet = workbook.CreateSheet("Global");

            var headerRow = sheet.CreateRow(0);
            headerRow.CreateCell(0).SetCellValue("");
            WriteStatisticsHeader(headerRow, 1);

            var parsingRow = sheet.CreateRow(1);
            parsingRow.CreateCell(0).SetCellValue("Parse Time");
            WriteStatistics(
                parsingRow,
                globalStatistics.ParseTimeStatistics,
                1,
                static d => Duration.Format((long) Math.Ceiling(d)));

            if (globalStatistics.AllocationStatistics is not null)
            {
                var allocationsRow = sheet.CreateRow(2);
                allocationsRow.CreateCell(0).SetCellValue("Allocations");
                WriteStatistics(
                    allocationsRow,
                    globalStatistics.AllocationStatistics,
                    1,
                    FileSize.Format);
            }

            if (globalStatistics.TokenCountStatistics is not null)
            {
                var tokenCountRow = sheet.CreateRow(3);
                tokenCountRow.CreateCell(0).SetCellValue("Token Count");
                WriteStatistics(tokenCountRow, globalStatistics.TokenCountStatistics, 1);
            }

            if (globalStatistics.TokenLengthStatistics is not null)
            {
                var tokenLengthRow = sheet.CreateRow(4);
                tokenLengthRow.CreateCell(1).SetCellValue("Token Lengths");
                WriteStatistics(tokenLengthRow, globalStatistics.TokenLengthStatistics, 1);
            }

            WriteFeatureStatistics(sheet, globalStatistics, totalFiles);
        }

        private static void WriteFeatureStatistics(ISheet sheet, GlobalStatistics globalStatistics, int totalFiles)
        {
            var featureStatistics = globalStatistics.FeatureStatistics;
            var featuresHeaderRow = sheet.CreateRow(6);
            WriteCells(featuresHeaderRow, 0,
                "Feature",
                "Files Using It",
                "Total Files",
                "Usage (%)");
            writeFS(7, "Binary Numbers", featureStatistics.HasBinaryNumbers);
            writeFS(8, "C Comments", featureStatistics.HasCComments);
            writeFS(9, "Compound Assignment", featureStatistics.HasCompoundAssignments);
            writeFS(10, "Empty Statements", featureStatistics.HasEmptyStatements);
            writeFS(11, "C Boolean Operators", featureStatistics.HasCBooleanOperators);
            writeFS(12, "Goto", featureStatistics.HasGoto);
            writeFS(13, "Hex Escapes", featureStatistics.HasHexEscapesInStrings);
            writeFS(14, "Hex Floats", featureStatistics.HasHexFloatLiterals);
            writeFS(15, "Octal Numbers", featureStatistics.HasOctalNumbers);
            writeFS(16, "Shebang", featureStatistics.HasShebang);
            writeFS(17, "Underscore in Numbers", featureStatistics.HasUnderscoreInNumericLiterals);
            writeFS(18, "LuaJIT Identifiers", featureStatistics.HasLuajitIdentifiers);

            void writeFS(int rowNum, string name, int featureNum)
            {
                WriteCells(sheet!.CreateRow(rowNum), 0,
                    name,
                    featureNum,
                    totalFiles,
                    $"{(double) featureNum / totalFiles:P}");
            }
        }

        private static void WriteBasicFileStatistics(XSSFWorkbook workbook, ImmutableArray<FileStatistics> fileStatistics)
        {
            var sheet = workbook.CreateSheet("Files");
            var headerRow = sheet.CreateRow(0);
            headerRow.CreateCell(0).SetCellValue("File Name");
            WriteCells(headerRow, 0,
                "File Name",
                "Parse Time",
                "Parse Time (Pretty)",
                "Allocation",
                "Allocation (Pretty)",
                "Token Count",
                "Errors",
                "Warnings",
                "Informations");

            for (var idx = 0; idx < fileStatistics.Length; idx++)
            {
                var statistic = fileStatistics[idx];
                var row = sheet.CreateRow(idx + 1);
                WriteCells(row, 0,
                    statistic.FileName,
                    statistic.ParseStatistics.ParseTime,
                    Duration.Format(statistic.ParseStatistics.ParseTime),
                    statistic.ParseStatistics.BytesAllocated,
                    FileSize.Format(statistic.ParseStatistics.BytesAllocated),
                    statistic.TokenStatistics?.TokenCount ?? 0,
                    statistic.DiagnosticStatistics.ErrorCount,
                    statistic.DiagnosticStatistics.WarningCount,
                    statistic.DiagnosticStatistics.InformationCount);
            }
        }

        private static void WriteTokenFileStatistics(XSSFWorkbook workbook, ImmutableArray<FileStatistics> fileStatistics)
        {
            if (!fileStatistics.Any(f => f.TokenStatistics is not null))
                return;

            var sheet = workbook.CreateSheet("Token Length");
            var headerRow = sheet.CreateRow(0);
            headerRow.CreateCell(0).SetCellValue("File Name");
            WriteStatisticsHeader(headerRow, 1);

            for (var idx = 0; idx < fileStatistics.Length; idx++)
            {
                var statistics = fileStatistics[idx];
                var tokenLengthStatistics = statistics.TokenStatistics?.TokenLengthStatistics;
                if (tokenLengthStatistics is null) continue;

                var row = sheet.CreateRow(idx + 1);
                row.CreateCell(0).SetCellValue(statistics.FileName);
                WriteStatistics(row, tokenLengthStatistics, 1);
            }
        }

        private static void WriteFeatureStatistics(XSSFWorkbook workbook, ImmutableArray<FileStatistics> fileStatistics)
        {
            if (!fileStatistics.Any(f => f.FeatureStatistics is not null))
                return;

            var sheet = workbook.CreateSheet("Feature Usage");
            var headerRow = sheet.CreateRow(0);
            WriteCells(headerRow, 0,
                "File Name",
                "Has Binary Numbers",
                "Has C Comments",
                "Has Compound Assignments",
                "Has Empty Statements",
                "Has Boolean C Operators",
                "Has Goto",
                "Has Hex Escapes in Strings",
                "Has Hex Floats",
                "Has Octal Numbers",
                "Has Shebang",
                "Has Underscore in Numeric Literals",
                "Has LuaJIT Identifiers",
                "Continue Type");

            for (var idx = 0; idx < fileStatistics.Length; idx++)
            {
                var statistics = fileStatistics[idx];
                var featureStatistics = statistics.FeatureStatistics;
                if (featureStatistics is null) continue;

                var row = sheet.CreateRow(idx + 1);
                WriteCells(row, 0,
                    statistics.FileName,
                    featureStatistics.HasBinaryNumbers,
                    featureStatistics.HasCComments,
                    featureStatistics.HasCompoundAssignments,
                    featureStatistics.HasEmptyStatements,
                    featureStatistics.HasCBooleanOperators,
                    featureStatistics.HasGoto,
                    featureStatistics.HasHexEscapesInStrings,
                    featureStatistics.HasHexFloatLiterals,
                    featureStatistics.HasOctalNumbers,
                    featureStatistics.HasShebang,
                    featureStatistics.HasUnderscoreInNumericLiterals,
                    featureStatistics.HasLuajitIdentifiers,
                    featureStatistics.ContinueType);
            }
        }
    }
}

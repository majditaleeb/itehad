using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Text;

/// <summary>
/// Minimal real-.xlsx generator (Office Open XML, no external libraries).
/// Produces right-to-left styled worksheets using the app's brand colors
/// (navy #12192C, gold #E29500). Dates/times/numbers are written as native
/// Excel values with number formats, so Excel shows them correctly.
/// </summary>
public sealed class Xlsx
{
    // Style indexes (must match cellXfs order in StylesXml below)
    public const int StDefault = 0;
    public const int StTitle = 1;      // big white bold on navy
    public const int StSubtitle = 2;   // gold on navy
    public const int StHeader = 3;     // bold navy on gold
    public const int StText = 4;       // bordered text
    public const int StTextZ = 5;      // bordered text, zebra
    public const int StDate = 6;       // yyyy-mm-dd hh:mm
    public const int StDateZ = 7;
    public const int StMoney = 8;      // #,##0.00
    public const int StMoneyZ = 9;
    public const int StSumLabel = 10;  // bold on light gold
    public const int StSumValue = 11;  // bold money on light gold
    public const int StCenter = 12;
    public const int StCenterZ = 13;
    public const int StDebit = 14;     // red money
    public const int StDebitZ = 15;
    public const int StCredit = 16;    // green money
    public const int StCreditZ = 17;
    public const int StSerial = 18;    // bold centered
    public const int StSerialZ = 19;
    public const int StTime = 20;      // hh:mm
    public const int StTimeZ = 21;
    public const int StBold = 22;
    public const int StSection = 23;   // section title: bold white on navy, smaller
    public const int StBalance = 24;   // bold money

    public struct Cell
    {
        public int Kind;      // 0=text, 1=number, 2=empty
        public string Text;
        public double Number;
        public int Style;
    }

    public static Cell T(string value, int style)
    {
        return new Cell { Kind = string.IsNullOrEmpty(value) ? 2 : 0, Text = value, Style = style };
    }

    public static Cell N(decimal? value, int style)
    {
        if (!value.HasValue) return new Cell { Kind = 2, Style = style };
        return new Cell { Kind = 1, Number = (double)value.Value, Style = style };
    }

    public static Cell D(DateTime? value, int style)
    {
        if (!value.HasValue) return new Cell { Kind = 2, Style = style };
        return new Cell { Kind = 1, Number = value.Value.ToOADate(), Style = style };
    }

    public static Cell E(int style)
    {
        return new Cell { Kind = 2, Style = style };
    }

    private readonly string _sheetName;
    private readonly StringBuilder _rows = new StringBuilder();
    private readonly List<string> _merges = new List<string>();
    private double[] _colWidths = new double[0];
    private int _rowIndex;
    private int _freezeAfterRow;

    public Xlsx(string sheetName)
    {
        _sheetName = sheetName;
    }

    public void SetColumns(params double[] widths) { _colWidths = widths; }

    public void FreezeAfterRow(int row) { _freezeAfterRow = row; }

    /// <summary>Adds a row; returns its 1-based row number.</summary>
    public int AddRow(double height, params Cell[] cells)
    {
        _rowIndex++;
        _rows.Append("<row r=\"").Append(_rowIndex).Append('"');
        if (height > 0)
            _rows.Append(" ht=\"").Append(height.ToString(CultureInfo.InvariantCulture)).Append("\" customHeight=\"1\"");
        _rows.Append('>');

        for (int i = 0; i < cells.Length; i++)
        {
            var c = cells[i];
            string cellRef = ColName(i + 1) + _rowIndex;
            if (c.Kind == 1)
            {
                _rows.Append("<c r=\"").Append(cellRef).Append("\" s=\"").Append(c.Style)
                     .Append("\"><v>").Append(c.Number.ToString("R", CultureInfo.InvariantCulture)).Append("</v></c>");
            }
            else if (c.Kind == 0)
            {
                _rows.Append("<c r=\"").Append(cellRef).Append("\" s=\"").Append(c.Style)
                     .Append("\" t=\"inlineStr\"><is><t xml:space=\"preserve\">").Append(Esc(c.Text)).Append("</t></is></c>");
            }
            else
            {
                _rows.Append("<c r=\"").Append(cellRef).Append("\" s=\"").Append(c.Style).Append("\"/>");
            }
        }
        _rows.Append("</row>");
        return _rowIndex;
    }

    public int AddRow(params Cell[] cells) { return AddRow(0, cells); }

    public void Merge(int row, int colFrom, int colTo)
    {
        _merges.Add(ColName(colFrom) + row + ":" + ColName(colTo) + row);
    }

    public byte[] Build()
    {
        using (var ms = new MemoryStream())
        {
            using (var zip = new ZipArchive(ms, ZipArchiveMode.Create, true))
            {
                AddEntry(zip, "[Content_Types].xml", ContentTypesXml);
                AddEntry(zip, "_rels/.rels", RelsXml);
                AddEntry(zip, "xl/workbook.xml", WorkbookXml());
                AddEntry(zip, "xl/_rels/workbook.xml.rels", WorkbookRelsXml);
                AddEntry(zip, "xl/styles.xml", StylesXml);
                AddEntry(zip, "xl/worksheets/sheet1.xml", SheetXml());
            }
            return ms.ToArray();
        }
    }

    private static void AddEntry(ZipArchive zip, string name, string content)
    {
        var entry = zip.CreateEntry(name, CompressionLevel.Optimal);
        using (var w = new StreamWriter(entry.Open(), new UTF8Encoding(false)))
        {
            w.Write(content);
        }
    }

    private static string ColName(int index)
    {
        string s = "";
        while (index > 0)
        {
            int m = (index - 1) % 26;
            s = (char)('A' + m) + s;
            index = (index - 1) / 26;
        }
        return s;
    }

    private static string Esc(string s)
    {
        if (string.IsNullOrEmpty(s)) return "";
        return s.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;").Replace("\"", "&quot;");
    }

    private string SheetXml()
    {
        var sb = new StringBuilder();
        sb.Append("<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?>");
        sb.Append("<worksheet xmlns=\"http://schemas.openxmlformats.org/spreadsheetml/2006/main\">");
        sb.Append("<sheetViews><sheetView workbookViewId=\"0\" rightToLeft=\"1\" showGridLines=\"0\">");
        if (_freezeAfterRow > 0)
        {
            sb.Append("<pane ySplit=\"").Append(_freezeAfterRow)
              .Append("\" topLeftCell=\"A").Append(_freezeAfterRow + 1)
              .Append("\" activePane=\"bottomLeft\" state=\"frozen\"/>");
        }
        sb.Append("</sheetView></sheetViews>");
        sb.Append("<sheetFormatPr defaultRowHeight=\"20\" customHeight=\"1\"/>");
        if (_colWidths.Length > 0)
        {
            sb.Append("<cols>");
            for (int i = 0; i < _colWidths.Length; i++)
            {
                sb.Append("<col min=\"").Append(i + 1).Append("\" max=\"").Append(i + 1)
                  .Append("\" width=\"").Append(_colWidths[i].ToString(CultureInfo.InvariantCulture))
                  .Append("\" customWidth=\"1\"/>");
            }
            sb.Append("</cols>");
        }
        sb.Append("<sheetData>").Append(_rows).Append("</sheetData>");
        if (_merges.Count > 0)
        {
            sb.Append("<mergeCells count=\"").Append(_merges.Count).Append("\">");
            foreach (var m in _merges) sb.Append("<mergeCell ref=\"").Append(m).Append("\"/>");
            sb.Append("</mergeCells>");
        }
        sb.Append("</worksheet>");
        return sb.ToString();
    }

    private string WorkbookXml()
    {
        return "<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?>" +
            "<workbook xmlns=\"http://schemas.openxmlformats.org/spreadsheetml/2006/main\" " +
            "xmlns:r=\"http://schemas.openxmlformats.org/officeDocument/2006/relationships\">" +
            "<sheets><sheet name=\"" + Esc(_sheetName) + "\" sheetId=\"1\" r:id=\"rId1\"/></sheets></workbook>";
    }

    private const string ContentTypesXml =
        "<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?>" +
        "<Types xmlns=\"http://schemas.openxmlformats.org/package/2006/content-types\">" +
        "<Default Extension=\"rels\" ContentType=\"application/vnd.openxmlformats-package.relationships+xml\"/>" +
        "<Default Extension=\"xml\" ContentType=\"application/xml\"/>" +
        "<Override PartName=\"/xl/workbook.xml\" ContentType=\"application/vnd.openxmlformats-officedocument.spreadsheetml.sheet.main+xml\"/>" +
        "<Override PartName=\"/xl/worksheets/sheet1.xml\" ContentType=\"application/vnd.openxmlformats-officedocument.spreadsheetml.worksheet+xml\"/>" +
        "<Override PartName=\"/xl/styles.xml\" ContentType=\"application/vnd.openxmlformats-officedocument.spreadsheetml.styles+xml\"/>" +
        "</Types>";

    private const string RelsXml =
        "<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?>" +
        "<Relationships xmlns=\"http://schemas.openxmlformats.org/package/2006/relationships\">" +
        "<Relationship Id=\"rId1\" Type=\"http://schemas.openxmlformats.org/officeDocument/2006/relationships/officeDocument\" Target=\"xl/workbook.xml\"/>" +
        "</Relationships>";

    private const string WorkbookRelsXml =
        "<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?>" +
        "<Relationships xmlns=\"http://schemas.openxmlformats.org/package/2006/relationships\">" +
        "<Relationship Id=\"rId1\" Type=\"http://schemas.openxmlformats.org/officeDocument/2006/relationships/worksheet\" Target=\"worksheets/sheet1.xml\"/>" +
        "<Relationship Id=\"rId2\" Type=\"http://schemas.openxmlformats.org/officeDocument/2006/relationships/styles\" Target=\"styles.xml\"/>" +
        "</Relationships>";

    // Brand palette: navy FF12192C, gold FFE29500, zebra FFF3F4F6, light gold FFFDF3E0
    private const string StylesXml =
        "<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?>" +
        "<styleSheet xmlns=\"http://schemas.openxmlformats.org/spreadsheetml/2006/main\">" +
        "<numFmts count=\"4\">" +
        "<numFmt numFmtId=\"164\" formatCode=\"yyyy\\-mm\\-dd\\ hh:mm\"/>" +
        "<numFmt numFmtId=\"165\" formatCode=\"#,##0.00\"/>" +
        "<numFmt numFmtId=\"166\" formatCode=\"#,##0\"/>" +
        "<numFmt numFmtId=\"167\" formatCode=\"hh:mm\"/>" +
        "</numFmts>" +
        "<fonts count=\"9\">" +
        "<font><sz val=\"11\"/><color rgb=\"FF1F2937\"/><name val=\"Calibri\"/></font>" +           // 0 body
        "<font><b/><sz val=\"16\"/><color rgb=\"FFFFFFFF\"/><name val=\"Calibri\"/></font>" +      // 1 title
        "<font><b/><sz val=\"11\"/><color rgb=\"FFE29500\"/><name val=\"Calibri\"/></font>" +      // 2 gold subtitle
        "<font><b/><sz val=\"11\"/><color rgb=\"FF12192C\"/><name val=\"Calibri\"/></font>" +      // 3 bold navy
        "<font><b/><sz val=\"11\"/><color rgb=\"FF1F2937\"/><name val=\"Calibri\"/></font>" +      // 4 bold body
        "<font><b/><sz val=\"11\"/><color rgb=\"FFB91C1C\"/><name val=\"Calibri\"/></font>" +      // 5 red bold
        "<font><b/><sz val=\"11\"/><color rgb=\"FF15803D\"/><name val=\"Calibri\"/></font>" +      // 6 green bold
        "<font><b/><sz val=\"13\"/><color rgb=\"FFFFFFFF\"/><name val=\"Calibri\"/></font>" +      // 7 section title
        "<font><sz val=\"10\"/><color rgb=\"FF6B7280\"/><name val=\"Calibri\"/></font>" +          // 8 muted
        "</fonts>" +
        "<fills count=\"7\">" +
        "<fill><patternFill patternType=\"none\"/></fill>" +
        "<fill><patternFill patternType=\"gray125\"/></fill>" +
        "<fill><patternFill patternType=\"solid\"><fgColor rgb=\"FF12192C\"/></patternFill></fill>" + // 2 navy
        "<fill><patternFill patternType=\"solid\"><fgColor rgb=\"FFE29500\"/></patternFill></fill>" + // 3 gold
        "<fill><patternFill patternType=\"solid\"><fgColor rgb=\"FFF3F4F6\"/></patternFill></fill>" + // 4 zebra
        "<fill><patternFill patternType=\"solid\"><fgColor rgb=\"FFFDF3E0\"/></patternFill></fill>" + // 5 light gold
        "<fill><patternFill patternType=\"solid\"><fgColor rgb=\"FFFFFFFF\"/></patternFill></fill>" + // 6 white
        "</fills>" +
        "<borders count=\"2\">" +
        "<border><left/><right/><top/><bottom/><diagonal/></border>" +
        "<border>" +
        "<left style=\"thin\"><color rgb=\"FFD8DCE0\"/></left>" +
        "<right style=\"thin\"><color rgb=\"FFD8DCE0\"/></right>" +
        "<top style=\"thin\"><color rgb=\"FFD8DCE0\"/></top>" +
        "<bottom style=\"thin\"><color rgb=\"FFD8DCE0\"/></bottom>" +
        "<diagonal/></border>" +
        "</borders>" +
        "<cellStyleXfs count=\"1\"><xf numFmtId=\"0\" fontId=\"0\" fillId=\"0\" borderId=\"0\"/></cellStyleXfs>" +
        "<cellXfs count=\"25\">" +
        // 0 default
        "<xf numFmtId=\"0\" fontId=\"0\" fillId=\"0\" borderId=\"0\" xfId=\"0\"/>" +
        // 1 title
        "<xf numFmtId=\"0\" fontId=\"1\" fillId=\"2\" borderId=\"0\" xfId=\"0\" applyFont=\"1\" applyFill=\"1\" applyAlignment=\"1\"><alignment horizontal=\"center\" vertical=\"center\"/></xf>" +
        // 2 subtitle
        "<xf numFmtId=\"0\" fontId=\"2\" fillId=\"2\" borderId=\"0\" xfId=\"0\" applyFont=\"1\" applyFill=\"1\" applyAlignment=\"1\"><alignment horizontal=\"center\" vertical=\"center\"/></xf>" +
        // 3 header
        "<xf numFmtId=\"0\" fontId=\"3\" fillId=\"3\" borderId=\"1\" xfId=\"0\" applyFont=\"1\" applyFill=\"1\" applyBorder=\"1\" applyAlignment=\"1\"><alignment horizontal=\"center\" vertical=\"center\" wrapText=\"1\"/></xf>" +
        // 4 text
        "<xf numFmtId=\"0\" fontId=\"0\" fillId=\"6\" borderId=\"1\" xfId=\"0\" applyFont=\"1\" applyFill=\"1\" applyBorder=\"1\" applyAlignment=\"1\"><alignment vertical=\"center\" wrapText=\"1\"/></xf>" +
        // 5 text zebra
        "<xf numFmtId=\"0\" fontId=\"0\" fillId=\"4\" borderId=\"1\" xfId=\"0\" applyFont=\"1\" applyFill=\"1\" applyBorder=\"1\" applyAlignment=\"1\"><alignment vertical=\"center\" wrapText=\"1\"/></xf>" +
        // 6 date
        "<xf numFmtId=\"164\" fontId=\"0\" fillId=\"6\" borderId=\"1\" xfId=\"0\" applyNumberFormat=\"1\" applyFont=\"1\" applyFill=\"1\" applyBorder=\"1\" applyAlignment=\"1\"><alignment horizontal=\"center\" vertical=\"center\"/></xf>" +
        // 7 date zebra
        "<xf numFmtId=\"164\" fontId=\"0\" fillId=\"4\" borderId=\"1\" xfId=\"0\" applyNumberFormat=\"1\" applyFont=\"1\" applyFill=\"1\" applyBorder=\"1\" applyAlignment=\"1\"><alignment horizontal=\"center\" vertical=\"center\"/></xf>" +
        // 8 money
        "<xf numFmtId=\"165\" fontId=\"0\" fillId=\"6\" borderId=\"1\" xfId=\"0\" applyNumberFormat=\"1\" applyFont=\"1\" applyFill=\"1\" applyBorder=\"1\" applyAlignment=\"1\"><alignment horizontal=\"center\" vertical=\"center\"/></xf>" +
        // 9 money zebra
        "<xf numFmtId=\"165\" fontId=\"0\" fillId=\"4\" borderId=\"1\" xfId=\"0\" applyNumberFormat=\"1\" applyFont=\"1\" applyFill=\"1\" applyBorder=\"1\" applyAlignment=\"1\"><alignment horizontal=\"center\" vertical=\"center\"/></xf>" +
        // 10 summary label
        "<xf numFmtId=\"0\" fontId=\"4\" fillId=\"5\" borderId=\"1\" xfId=\"0\" applyFont=\"1\" applyFill=\"1\" applyBorder=\"1\" applyAlignment=\"1\"><alignment vertical=\"center\"/></xf>" +
        // 11 summary value
        "<xf numFmtId=\"165\" fontId=\"4\" fillId=\"5\" borderId=\"1\" xfId=\"0\" applyNumberFormat=\"1\" applyFont=\"1\" applyFill=\"1\" applyBorder=\"1\" applyAlignment=\"1\"><alignment horizontal=\"center\" vertical=\"center\"/></xf>" +
        // 12 center text
        "<xf numFmtId=\"0\" fontId=\"0\" fillId=\"6\" borderId=\"1\" xfId=\"0\" applyFont=\"1\" applyFill=\"1\" applyBorder=\"1\" applyAlignment=\"1\"><alignment horizontal=\"center\" vertical=\"center\"/></xf>" +
        // 13 center text zebra
        "<xf numFmtId=\"0\" fontId=\"0\" fillId=\"4\" borderId=\"1\" xfId=\"0\" applyFont=\"1\" applyFill=\"1\" applyBorder=\"1\" applyAlignment=\"1\"><alignment horizontal=\"center\" vertical=\"center\"/></xf>" +
        // 14 debit red
        "<xf numFmtId=\"165\" fontId=\"5\" fillId=\"6\" borderId=\"1\" xfId=\"0\" applyNumberFormat=\"1\" applyFont=\"1\" applyFill=\"1\" applyBorder=\"1\" applyAlignment=\"1\"><alignment horizontal=\"center\" vertical=\"center\"/></xf>" +
        // 15 debit red zebra
        "<xf numFmtId=\"165\" fontId=\"5\" fillId=\"4\" borderId=\"1\" xfId=\"0\" applyNumberFormat=\"1\" applyFont=\"1\" applyFill=\"1\" applyBorder=\"1\" applyAlignment=\"1\"><alignment horizontal=\"center\" vertical=\"center\"/></xf>" +
        // 16 credit green
        "<xf numFmtId=\"165\" fontId=\"6\" fillId=\"6\" borderId=\"1\" xfId=\"0\" applyNumberFormat=\"1\" applyFont=\"1\" applyFill=\"1\" applyBorder=\"1\" applyAlignment=\"1\"><alignment horizontal=\"center\" vertical=\"center\"/></xf>" +
        // 17 credit green zebra
        "<xf numFmtId=\"165\" fontId=\"6\" fillId=\"4\" borderId=\"1\" xfId=\"0\" applyNumberFormat=\"1\" applyFont=\"1\" applyFill=\"1\" applyBorder=\"1\" applyAlignment=\"1\"><alignment horizontal=\"center\" vertical=\"center\"/></xf>" +
        // 18 serial
        "<xf numFmtId=\"166\" fontId=\"4\" fillId=\"6\" borderId=\"1\" xfId=\"0\" applyNumberFormat=\"1\" applyFont=\"1\" applyFill=\"1\" applyBorder=\"1\" applyAlignment=\"1\"><alignment horizontal=\"center\" vertical=\"center\"/></xf>" +
        // 19 serial zebra
        "<xf numFmtId=\"166\" fontId=\"4\" fillId=\"4\" borderId=\"1\" xfId=\"0\" applyNumberFormat=\"1\" applyFont=\"1\" applyFill=\"1\" applyBorder=\"1\" applyAlignment=\"1\"><alignment horizontal=\"center\" vertical=\"center\"/></xf>" +
        // 20 time
        "<xf numFmtId=\"167\" fontId=\"0\" fillId=\"6\" borderId=\"1\" xfId=\"0\" applyNumberFormat=\"1\" applyFont=\"1\" applyFill=\"1\" applyBorder=\"1\" applyAlignment=\"1\"><alignment horizontal=\"center\" vertical=\"center\"/></xf>" +
        // 21 time zebra
        "<xf numFmtId=\"167\" fontId=\"0\" fillId=\"4\" borderId=\"1\" xfId=\"0\" applyNumberFormat=\"1\" applyFont=\"1\" applyFill=\"1\" applyBorder=\"1\" applyAlignment=\"1\"><alignment horizontal=\"center\" vertical=\"center\"/></xf>" +
        // 22 bold text
        "<xf numFmtId=\"0\" fontId=\"4\" fillId=\"6\" borderId=\"1\" xfId=\"0\" applyFont=\"1\" applyFill=\"1\" applyBorder=\"1\" applyAlignment=\"1\"><alignment vertical=\"center\"/></xf>" +
        // 23 section title
        "<xf numFmtId=\"0\" fontId=\"7\" fillId=\"2\" borderId=\"0\" xfId=\"0\" applyFont=\"1\" applyFill=\"1\" applyAlignment=\"1\"><alignment horizontal=\"center\" vertical=\"center\"/></xf>" +
        // 24 balance bold money
        "<xf numFmtId=\"165\" fontId=\"4\" fillId=\"6\" borderId=\"1\" xfId=\"0\" applyNumberFormat=\"1\" applyFont=\"1\" applyFill=\"1\" applyBorder=\"1\" applyAlignment=\"1\"><alignment horizontal=\"center\" vertical=\"center\"/></xf>" +
        "</cellXfs>" +
        "<cellStyles count=\"1\"><cellStyle name=\"Normal\" xfId=\"0\" builtinId=\"0\"/></cellStyles>" +
        "</styleSheet>";
}

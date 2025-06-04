import * as XLSX from 'xlsx';
import { saveAs } from 'file-saver';
import { jsPDF } from 'jspdf';
import 'jspdf-autotable';
import _ from 'lodash';

interface DataTableColumn {
  key: string;
  title: string;
  dataIndex: string;
  exportFormatter?: (value: any) => string;
  exportable?: boolean;
}

export class ExportService {
  static exportCSV(data: any[], columns: DataTableColumn[], filename: string = 'data') {
    const exportableColumns = columns.filter(col => col.exportable !== false);
    const headers = exportableColumns.map(col => col.title).join(',');
    const rows = data.map(row => 
      exportableColumns.map(col => {
        const value = _.get(row, col.dataIndex);
        const exportValue = col.exportFormatter ? col.exportFormatter(value) : value;
        // Escape commas and quotes for CSV
        return typeof exportValue === 'string' && exportValue.includes(',') 
          ? `"${exportValue.replace(/"/g, '""')}"` 
          : exportValue;
      }).join(',')
    ).join('\n');
    
    const csv = `${headers}\n${rows}`;
    const blob = new Blob([csv], { type: 'text/csv;charset=utf-8;' });
    saveAs(blob, `${filename}.csv`);
  }

  static exportExcel(data: any[], columns: DataTableColumn[], filename: string = 'data') {
    const exportableColumns = columns.filter(col => col.exportable !== false);
    const worksheetData = data.map(row => {
      const exportRow: any = {};
      exportableColumns.forEach(col => {
        const value = _.get(row, col.dataIndex);
        exportRow[col.title] = col.exportFormatter ? col.exportFormatter(value) : value;
      });
      return exportRow;
    });
    
    const ws = XLSX.utils.json_to_sheet(worksheetData);
    const wb = XLSX.utils.book_new();
    XLSX.utils.book_append_sheet(wb, ws, 'Data');
    XLSX.writeFile(wb, `${filename}.xlsx`);
  }

  static exportPDF(data: any[], columns: DataTableColumn[], filename: string = 'data') {
    const doc = new jsPDF();
    const exportableColumns = columns.filter(col => col.exportable !== false);
    const headers = exportableColumns.map(col => col.title);
    const rows = data.map(row => 
      exportableColumns.map(col => {
        const value = _.get(row, col.dataIndex);
        const exportValue = col.exportFormatter ? col.exportFormatter(value) : String(value);
        return exportValue;
      })
    );
    
    (doc as any).autoTable({
      head: [headers],
      body: rows,
      theme: 'grid',
      styles: { fontSize: 8 },
      margin: { top: 20 },
      didDrawPage: (data: any) => {
        // Add title
        doc.setFontSize(16);
        doc.text('Data Export', 14, 15);
      }
    });
    
    doc.save(`${filename}.pdf`);
  }

  static exportJSON(data: any[], filename: string = 'data') {
    const json = JSON.stringify(data, null, 2);
    const blob = new Blob([json], { type: 'application/json' });
    saveAs(blob, `${filename}.json`);
  }

  static exportXML(data: any[], filename: string = 'data') {
    const xmlString = this.arrayToXML(data);
    const blob = new Blob([xmlString], { type: 'text/xml' });
    saveAs(blob, `${filename}.xml`);
  }

  static exportSQL(data: any[], tableName: string = 'data_table', filename: string = 'data') {
    if (data.length === 0) return;
    
    const columns = Object.keys(data[0]);
    const createTable = `CREATE TABLE ${tableName} (\n  ${columns.map(col => `${col} VARCHAR(255)`).join(',\n  ')}\n);\n\n`;
    
    const insertStatements = data.map(row => {
      const values = columns.map(col => {
        const value = row[col];
        return value === null || value === undefined ? 'NULL' : `'${String(value).replace(/'/g, "''")}'`;
      }).join(', ');
      return `INSERT INTO ${tableName} (${columns.join(', ')}) VALUES (${values});`;
    }).join('\n');
    
    const sql = createTable + insertStatements;
    const blob = new Blob([sql], { type: 'text/sql' });
    saveAs(blob, `${filename}.sql`);
  }

  private static arrayToXML(data: any[]): string {
    let xml = '<?xml version="1.0" encoding="UTF-8"?>\n<data>\n';
    
    data.forEach((item, index) => {
      xml += `  <record id="${index + 1}">\n`;
      Object.entries(item).forEach(([key, value]) => {
        xml += `    <${key}>${this.escapeXML(String(value))}</${key}>\n`;
      });
      xml += '  </record>\n';
    });
    
    xml += '</data>';
    return xml;
  }

  private static escapeXML(str: string): string {
    return str
      .replace(/&/g, '&amp;')
      .replace(/</g, '&lt;')
      .replace(/>/g, '&gt;')
      .replace(/"/g, '&quot;')
      .replace(/'/g, '&apos;');
  }
}

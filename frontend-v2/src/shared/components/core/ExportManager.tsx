import React, { useState } from 'react'
import { Button, Dropdown, Modal, Form, Input, Select, Space, message, Progress } from 'antd'
import {
  DownloadOutlined,
  FileExcelOutlined,
  FilePdfOutlined,
  FileTextOutlined,
  SettingOutlined
} from '@ant-design/icons'
import * as XLSX from 'xlsx'
import jsPDF from 'jspdf'
import 'jspdf-autotable'
import { saveAs } from 'file-saver'

// Extend jsPDF type to include autoTable
declare module 'jspdf' {
  interface jsPDF {
    autoTable: (options: any) => jsPDF
  }
}

interface ExportData {
  headers: string[]
  rows: any[][]
  title?: string
  metadata?: Record<string, any>
}

interface ExportOptions {
  format: 'csv' | 'excel' | 'pdf'
  filename?: string
  includeHeaders?: boolean
  includeMetadata?: boolean
  pageSize?: 'A4' | 'A3' | 'Letter'
  orientation?: 'portrait' | 'landscape'
  compression?: boolean
}

interface ExportManagerProps {
  data: ExportData
  loading?: boolean
  disabled?: boolean
  buttonText?: string
  showAdvancedOptions?: boolean
}

export const ExportManager: React.FC<ExportManagerProps> = ({
  data,
  loading = false,
  disabled = false,
  buttonText = 'Export',
  showAdvancedOptions = true
}) => {
  const [isModalOpen, setIsModalOpen] = useState(false)
  const [exportProgress, setExportProgress] = useState(0)
  const [isExporting, setIsExporting] = useState(false)
  const [form] = Form.useForm()

  const exportToCSV = async (options: ExportOptions): Promise<void> => {
    const { headers, rows } = data
    const { filename = 'export.csv', includeHeaders = true } = options

    let csvContent = ''
    
    // Add headers
    if (includeHeaders) {
      csvContent += headers.map(header => `"${header}"`).join(',') + '\n'
    }
    
    // Add rows
    rows.forEach((row, index) => {
      const csvRow = row.map(cell => {
        const cellValue = cell === null || cell === undefined ? '' : String(cell)
        return `"${cellValue.replace(/"/g, '""')}"`
      }).join(',')
      csvContent += csvRow + '\n'
      
      // Update progress
      setExportProgress(Math.round(((index + 1) / rows.length) * 100))
    })

    // Create and download file
    const blob = new Blob([csvContent], { type: 'text/csv;charset=utf-8;' })
    const link = document.createElement('a')
    link.href = URL.createObjectURL(blob)
    link.download = filename
    link.click()
    URL.revokeObjectURL(link.href)
  }

  const exportToExcel = async (options: ExportOptions): Promise<void> => {
    const { headers, rows } = data
    const { filename = 'export.xlsx', includeHeaders = true, includeMetadata = false } = options

    try {
      setExportProgress(10)

      // Create workbook data
      const worksheetData = []
      if (includeHeaders) {
        worksheetData.push(headers)
      }
      worksheetData.push(...rows)

      setExportProgress(30)

      // Create worksheet
      const worksheet = XLSX.utils.aoa_to_sheet(worksheetData)

      // Set column widths
      const columnWidths = headers.map(() => ({ wch: 15 }))
      worksheet['!cols'] = columnWidths

      setExportProgress(50)

      // Create workbook
      const workbook = XLSX.utils.book_new()
      XLSX.utils.book_append_sheet(workbook, worksheet, 'Data')

      setExportProgress(70)

      // Add metadata sheet if requested
      if (includeMetadata && data.metadata) {
        const metadataData = Object.entries(data.metadata).map(([key, value]) => [key, value])
        metadataData.unshift(['Property', 'Value'])
        const metadataSheet = XLSX.utils.aoa_to_sheet(metadataData)
        XLSX.utils.book_append_sheet(workbook, metadataSheet, 'Metadata')
      }

      setExportProgress(90)

      // Generate Excel file
      const excelBuffer = XLSX.write(workbook, { bookType: 'xlsx', type: 'array' })
      const blob = new Blob([excelBuffer], { type: 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet' })

      // Save file
      saveAs(blob, filename)
      setExportProgress(100)

    } catch (error) {
      console.error('Excel export error:', error)
      message.error('Excel export failed')
      throw error
    }
  }

  const exportToPDF = async (options: ExportOptions): Promise<void> => {
    const { headers, rows } = data
    const {
      filename = 'export.pdf',
      includeHeaders = true,
      pageSize = 'A4',
      orientation = 'portrait',
      includeMetadata = false
    } = options

    try {
      setExportProgress(10)

      // Create PDF document
      const doc = new jsPDF({
        orientation,
        unit: 'mm',
        format: pageSize.toLowerCase() as any
      })

      setExportProgress(20)

      let currentY = 20

      // Add title
      if (data.title) {
        doc.setFontSize(16)
        doc.setFont('helvetica', 'bold')
        doc.text(data.title, 20, currentY)
        currentY += 15
      }

      setExportProgress(40)

      // Add metadata if requested
      if (includeMetadata && data.metadata) {
        doc.setFontSize(10)
        doc.setFont('helvetica', 'normal')
        doc.text('Export Information:', 20, currentY)
        currentY += 8

        Object.entries(data.metadata).forEach(([key, value]) => {
          doc.text(`${key}: ${value}`, 25, currentY)
          currentY += 5
        })

        currentY += 10
      }

      setExportProgress(60)

      // Add table
      doc.autoTable({
        head: includeHeaders ? [headers] : undefined,
        body: rows,
        startY: currentY,
        styles: {
          fontSize: 8,
          cellPadding: 2
        },
        headStyles: {
          fillColor: [66, 139, 202],
          textColor: [255, 255, 255],
          fontStyle: 'bold'
        },
        alternateRowStyles: {
          fillColor: [245, 245, 245]
        },
        margin: { top: 20, right: 20, bottom: 20, left: 20 },
        theme: 'striped'
      })

      setExportProgress(90)

      // Add footer
      const pageCount = doc.getNumberOfPages()
      for (let i = 1; i <= pageCount; i++) {
        doc.setPage(i)
        doc.setFontSize(8)
        doc.setFont('helvetica', 'normal')
        doc.text(
          `Page ${i} of ${pageCount} - Generated on ${new Date().toLocaleString()}`,
          20,
          doc.internal.pageSize.height - 10
        )
      }

      // Save file
      doc.save(filename)
      setExportProgress(100)

    } catch (error) {
      console.error('PDF export error:', error)
      message.error('PDF export failed')
      throw error
    }
  }

  const handleExport = async (format: 'csv' | 'excel' | 'pdf', customOptions?: Partial<ExportOptions>) => {
    if (!data.rows.length) {
      message.warning('No data to export')
      return
    }

    setIsExporting(true)
    setExportProgress(0)

    const defaultOptions: ExportOptions = {
      format,
      filename: `export_${new Date().toISOString().split('T')[0]}.${format === 'excel' ? 'xlsx' : format}`,
      includeHeaders: true,
      includeMetadata: false,
      pageSize: 'A4',
      orientation: 'portrait',
      compression: false,
      ...customOptions
    }

    try {
      switch (format) {
        case 'csv':
          await exportToCSV(defaultOptions)
          break
        case 'excel':
          await exportToExcel(defaultOptions)
          break
        case 'pdf':
          await exportToPDF(defaultOptions)
          break
      }
      
      message.success(`${format.toUpperCase()} export completed successfully`)
    } catch (error) {
      console.error('Export failed:', error)
      message.error(`${format.toUpperCase()} export failed`)
    } finally {
      setIsExporting(false)
      setExportProgress(0)
      setIsModalOpen(false)
    }
  }

  const handleAdvancedExport = async (values: any) => {
    await handleExport(values.format, {
      filename: values.filename,
      includeHeaders: values.includeHeaders,
      includeMetadata: values.includeMetadata,
      pageSize: values.pageSize,
      orientation: values.orientation,
      compression: values.compression
    })
  }

  const exportMenuItems = [
    {
      key: 'csv',
      label: (
        <Space>
          <FileTextOutlined />
          Export as CSV
        </Space>
      ),
      onClick: () => handleExport('csv')
    },
    {
      key: 'excel',
      label: (
        <Space>
          <FileExcelOutlined />
          Export as Excel
        </Space>
      ),
      onClick: () => handleExport('excel')
    },
    {
      key: 'pdf',
      label: (
        <Space>
          <FilePdfOutlined />
          Export as PDF
        </Space>
      ),
      onClick: () => handleExport('pdf')
    }
  ]

  if (showAdvancedOptions) {
    exportMenuItems.push({
      key: 'advanced',
      label: (
        <Space>
          <SettingOutlined />
          Advanced Options
        </Space>
      ),
      onClick: () => setIsModalOpen(true)
    })
  }

  return (
    <>
      <Dropdown
        menu={{ items: exportMenuItems }}
        placement="bottomRight"
        disabled={disabled || loading || isExporting}
      >
        <Button 
          icon={<DownloadOutlined />}
          loading={loading || isExporting}
          disabled={disabled}
        >
          {buttonText}
        </Button>
      </Dropdown>

      {/* Export Progress */}
      {isExporting && (
        <div style={{ 
          position: 'fixed', 
          top: '50%', 
          left: '50%', 
          transform: 'translate(-50%, -50%)',
          zIndex: 1000,
          backgroundColor: 'white',
          padding: '20px',
          borderRadius: '8px',
          boxShadow: '0 4px 12px rgba(0, 0, 0, 0.15)',
          minWidth: '300px'
        }}>
          <div style={{ textAlign: 'center', marginBottom: '16px' }}>
            Exporting data...
          </div>
          <Progress percent={exportProgress} status="active" />
        </div>
      )}

      {/* Advanced Export Modal */}
      <Modal
        title="Advanced Export Options"
        open={isModalOpen}
        onCancel={() => setIsModalOpen(false)}
        footer={null}
        width={500}
      >
        <Form
          form={form}
          layout="vertical"
          onFinish={handleAdvancedExport}
          initialValues={{
            format: 'csv',
            filename: `export_${new Date().toISOString().split('T')[0]}`,
            includeHeaders: true,
            includeMetadata: false,
            pageSize: 'A4',
            orientation: 'portrait',
            compression: false
          }}
        >
          <Form.Item
            name="format"
            label="Export Format"
            rules={[{ required: true }]}
          >
            <Select>
              <Select.Option value="csv">CSV</Select.Option>
              <Select.Option value="excel">Excel (XLSX)</Select.Option>
              <Select.Option value="pdf">PDF</Select.Option>
            </Select>
          </Form.Item>

          <Form.Item
            name="filename"
            label="Filename"
            rules={[{ required: true, message: 'Please enter filename' }]}
          >
            <Input placeholder="Enter filename without extension" />
          </Form.Item>

          <Form.Item name="includeHeaders" valuePropName="checked">
            <Select>
              <Select.Option value={true}>Include Headers</Select.Option>
              <Select.Option value={false}>Data Only</Select.Option>
            </Select>
          </Form.Item>

          <Form.Item name="includeMetadata" valuePropName="checked">
            <Select>
              <Select.Option value={true}>Include Metadata</Select.Option>
              <Select.Option value={false}>Data Only</Select.Option>
            </Select>
          </Form.Item>

          <Form.Item
            noStyle
            shouldUpdate={(prevValues, currentValues) => prevValues.format !== currentValues.format}
          >
            {({ getFieldValue }) => {
              const format = getFieldValue('format')
              return format === 'pdf' ? (
                <>
                  <Form.Item name="pageSize" label="Page Size">
                    <Select>
                      <Select.Option value="A4">A4</Select.Option>
                      <Select.Option value="A3">A3</Select.Option>
                      <Select.Option value="Letter">Letter</Select.Option>
                    </Select>
                  </Form.Item>

                  <Form.Item name="orientation" label="Orientation">
                    <Select>
                      <Select.Option value="portrait">Portrait</Select.Option>
                      <Select.Option value="landscape">Landscape</Select.Option>
                    </Select>
                  </Form.Item>
                </>
              ) : null
            }}
          </Form.Item>

          <Form.Item style={{ textAlign: 'right', marginBottom: 0 }}>
            <Space>
              <Button onClick={() => setIsModalOpen(false)}>
                Cancel
              </Button>
              <Button type="primary" htmlType="submit" loading={isExporting}>
                Export
              </Button>
            </Space>
          </Form.Item>
        </Form>
      </Modal>
    </>
  )
}

// Hook for export functionality
export const useExport = () => {
  const [isExporting, setIsExporting] = useState(false)
  const [progress, setProgress] = useState(0)

  const exportData = async (data: ExportData, options: ExportOptions) => {
    setIsExporting(true)
    setProgress(0)

    try {
      // Export logic would go here
      // This is a simplified version
      await new Promise(resolve => setTimeout(resolve, 1000))
      setProgress(100)
      message.success('Export completed')
    } catch (error) {
      message.error('Export failed')
      throw error
    } finally {
      setIsExporting(false)
      setProgress(0)
    }
  }

  return {
    exportData,
    isExporting,
    progress
  }
}

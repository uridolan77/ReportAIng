import React from 'react'
import { Card, Table, Button, Space, Tag, Typography } from 'antd'
import { PlayCircleOutlined, StarOutlined, DeleteOutlined } from '@ant-design/icons'
import { PageLayout } from '@shared/components/core/Layout'
import { useGetQueryHistoryQuery } from '@shared/store/api/queryApi'

const { Text } = Typography

export default function QueryHistory() {
  const { data: historyData, isLoading } = useGetQueryHistoryQuery({ page: 1, limit: 50 })

  const columns = [
    {
      title: 'Question',
      dataIndex: 'question',
      key: 'question',
      ellipsis: true,
      render: (text: string) => (
        <Text style={{ maxWidth: 300 }} ellipsis={{ tooltip: text }}>
          {text}
        </Text>
      ),
    },
    {
      title: 'Executed At',
      dataIndex: 'executedAt',
      key: 'executedAt',
      width: 180,
      render: (date: string) => new Date(date).toLocaleString(),
    },
    {
      title: 'Execution Time',
      dataIndex: 'executionTime',
      key: 'executionTime',
      width: 120,
      render: (time: number) => `${time}ms`,
    },
    {
      title: 'Rows',
      dataIndex: 'rowCount',
      key: 'rowCount',
      width: 80,
    },
    {
      title: 'Status',
      dataIndex: 'isFavorite',
      key: 'status',
      width: 100,
      render: (isFavorite: boolean) => (
        <Tag color={isFavorite ? 'gold' : 'default'}>
          {isFavorite ? 'Favorite' : 'Normal'}
        </Tag>
      ),
    },
    {
      title: 'Actions',
      key: 'actions',
      width: 150,
      render: (record: any) => (
        <Space>
          <Button size="small" icon={<PlayCircleOutlined />} title="Re-run" />
          <Button size="small" icon={<StarOutlined />} title="Add to favorites" />
          <Button size="small" icon={<DeleteOutlined />} danger title="Delete" />
        </Space>
      ),
    },
  ]

  return (
    <PageLayout
      title="Query History"
      subtitle="View and manage your previous queries"
    >
      <Card>
        <Table
          columns={columns}
          dataSource={historyData?.queries || []}
          loading={isLoading}
          rowKey="id"
          pagination={{
            total: historyData?.total || 0,
            pageSize: 50,
            showSizeChanger: true,
            showQuickJumper: true,
            showTotal: (total, range) =>
              `${range[0]}-${range[1]} of ${total} queries`,
          }}
        />
      </Card>
    </PageLayout>
  )
}

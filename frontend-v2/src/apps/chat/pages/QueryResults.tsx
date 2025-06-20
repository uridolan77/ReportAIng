import React from 'react'
import { Card, Typography } from 'antd'
import { PageLayout } from '@shared/components/core/Layout'
import { useParams } from 'react-router-dom'

const { Text } = Typography

export default function QueryResults() {
  const { queryId } = useParams<{ queryId: string }>()

  return (
    <PageLayout
      title="Query Results"
      subtitle={queryId ? `Results for query ${queryId}` : 'No query selected'}
    >
      <Card>
        <Text>Query results will be displayed here.</Text>
        {queryId && <Text>Query ID: {queryId}</Text>}
      </Card>
    </PageLayout>
  )
}

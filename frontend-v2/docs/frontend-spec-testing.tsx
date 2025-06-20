// features/cost-management/components/__tests__/CostDashboard.test.tsx
import { render, screen } from '@testing-library/react'
import { Provider } from 'react-redux'
import { CostDashboard } from '../CostDashboard'

const mockStore = createMockStore({
  api: {
    queries: {
      'getCostAnalytics(undefined)': {
        status: 'fulfilled',
        data: mockCostAnalytics
      }
    }
  }
})

test('renders cost dashboard with metrics', () => {
  render(
    <Provider store={mockStore}>
      <CostDashboard />
    </Provider>
  )
  
  expect(screen.getByText('Total Cost (Month)')).toBeInTheDocument()
  expect(screen.getByText('$1,234.56')).toBeInTheDocument()
})

// services/api/__tests__/costManagementApi.test.ts
import { costManagementApi } from '../costManagementApi'
import { setupApiStore } from '../../test-utils'

test('getCostAnalytics returns analytics data', async () => {
  const storeRef = setupApiStore(costManagementApi)
  
  const result = await storeRef.store.dispatch(
    costManagementApi.endpoints.getCostAnalytics.initiate({})
  )
  
  expect(result.data).toMatchObject({
    totalCost: expect.any(Number),
    dailyCost: expect.any(Number),
    trends: expect.any(Array)
  })
})

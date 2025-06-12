import type { Preview } from '@storybook/react-webpack5';
import { ConfigProvider } from 'antd';
import { BrowserRouter } from 'react-router-dom';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import '../src/App.css';

// Create a query client for Storybook
const queryClient = new QueryClient({
  defaultOptions: {
    queries: { retry: false },
    mutations: { retry: false }
  }
});

const preview: Preview = {
  parameters: {
    actions: { argTypesRegex: '^on[A-Z].*' },
    controls: {
      matchers: {
        color: /(background|color)$/i,
        date: /Date$/i,
      },
    },
    docs: {
      toc: true,
    },
    backgrounds: {
      default: 'light',
      values: [
        {
          name: 'light',
          value: '#ffffff',
        },
        {
          name: 'dark',
          value: '#333333',
        },
        {
          name: 'blue',
          value: '#1890ff',
        },
      ],
    },
    viewport: {
      viewports: {
        mobile: {
          name: 'Mobile',
          styles: {
            width: '375px',
            height: '667px',
          },
        },
        tablet: {
          name: 'Tablet',
          styles: {
            width: '768px',
            height: '1024px',
          },
        },
        desktop: {
          name: 'Desktop',
          styles: {
            width: '1200px',
            height: '800px',
          },
        },
        wide: {
          name: 'Wide Screen',
          styles: {
            width: '1920px',
            height: '1080px',
          },
        },
      },
    },
  },
  decorators: [
    (Story) => (
      <BrowserRouter>
        <QueryClientProvider client={queryClient}>
          <ConfigProvider>
            <div style={{ padding: '20px' }}>
              <Story />
            </div>
          </ConfigProvider>
        </QueryClientProvider>
      </BrowserRouter>
    ),
  ],
};

export default preview;
import React from 'react';
import { render, RenderOptions } from '@testing-library/react';
import { ConfigProvider } from 'antd';
import { BrowserRouter } from 'react-router-dom';
import { ErrorBoundary } from './components/ErrorBoundary/ErrorBoundary';

interface AllProvidersProps {
  children: React.ReactNode;
}

const AllProviders: React.FC<AllProvidersProps> = ({ children }) => {
  return (
    <ErrorBoundary>
      <ConfigProvider>
        <BrowserRouter>
          {children}
        </BrowserRouter>
      </ConfigProvider>
    </ErrorBoundary>
  );
};

const customRender = (
  ui: React.ReactElement,
  options?: Omit<RenderOptions, 'wrapper'>
) => render(ui, { wrapper: AllProviders, ...options });

export * from '@testing-library/react';
export { customRender as render };

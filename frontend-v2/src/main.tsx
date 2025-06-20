import React from 'react'
import ReactDOM from 'react-dom/client'
import { Provider } from 'react-redux'
import { BrowserRouter } from 'react-router-dom'
import { ConfigProvider } from 'antd'
import { PersistGate } from 'redux-persist/integration/react'
import { store, persistor } from '@shared/store'
import { antdTheme } from '@shared/theme'
import { LoadingSpinner } from '@shared/components/LoadingSpinner'
import App from './App'
import './index.css'

ReactDOM.createRoot(document.getElementById('root')!).render(
  <React.StrictMode>
    <Provider store={store}>
      <PersistGate loading={<LoadingSpinner fullScreen />} persistor={persistor}>
        <BrowserRouter>
          <ConfigProvider theme={antdTheme}>
            <App />
          </ConfigProvider>
        </BrowserRouter>
      </PersistGate>
    </Provider>
  </React.StrictMode>,
)

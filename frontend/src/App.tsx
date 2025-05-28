import React from 'react';
import { BrowserRouter as Router, Routes, Route, Navigate } from 'react-router-dom';
import { ConfigProvider, theme } from 'antd';
import { QueryInterface } from './components/QueryInterface/QueryInterface';
import EnhancedQueryBuilder from './components/QueryInterface/EnhancedQueryBuilder';
import UserContextPanel from './components/AI/UserContextPanel';
import QuerySimilarityAnalyzer from './components/AI/QuerySimilarityAnalyzer';
import AdvancedVisualizationPanel from './components/Visualization/AdvancedVisualizationPanel';
import AdvancedVisualizationDemo from './components/Demo/AdvancedVisualizationDemo';
import { Layout } from './components/Layout/Layout';
import { Login } from './components/Auth/Login';
import { useAuthStore } from './stores/authStore';
import './App.css';

const App: React.FC = () => {
  const { isAuthenticated } = useAuthStore();

  return (
    <ConfigProvider
      theme={{
        algorithm: theme.defaultAlgorithm,
        token: {
          colorPrimary: '#1890ff',
          borderRadius: 6,
        },
      }}
    >
      <Router>
        <div className="App">
          {isAuthenticated ? (
            <Layout>
              <Routes>
                <Route path="/" element={<QueryInterface />} />
                <Route path="/query" element={<QueryInterface />} />
                <Route path="/enhanced-query" element={<EnhancedQueryBuilder />} />
                <Route path="/ai-profile" element={<UserContextPanel />} />
                <Route path="/similarity" element={<QuerySimilarityAnalyzer />} />
                <Route
                  path="/advanced-viz"
                  element={
                    <AdvancedVisualizationPanel
                      data={[]}
                      columns={[]}
                      query=""
                    />
                  }
                />
                <Route path="/demo" element={<AdvancedVisualizationDemo />} />
                <Route path="*" element={<Navigate to="/" replace />} />
              </Routes>
            </Layout>
          ) : (
            <Routes>
              <Route path="/login" element={<Login />} />
              <Route path="*" element={<Navigate to="/login" replace />} />
            </Routes>
          )}
        </div>
      </Router>
    </ConfigProvider>
  );
};

export default App;

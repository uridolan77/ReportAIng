import React, { useEffect, useState } from 'react';
import { Card, Typography, Space, Button, Alert, Collapse } from 'antd';
import { useAuthStore } from '../../stores/authStore';
import { SecurityUtils } from '../../utils/security';

const { Text, Title } = Typography;

export const AuthDebugger: React.FC = () => {
  const [debugInfo, setDebugInfo] = useState<any>({});
  const [consoleLogs, setConsoleLogs] = useState<string[]>([]);
  const [preventRedirects, setPreventRedirects] = useState(false);
  const authState = useAuthStore();

  // Prevent redirects when debugging
  useEffect(() => {
    if (preventRedirects) {
      // Store original window.location.href
      const originalLocationHref = Object.getOwnPropertyDescriptor(window.location, 'href') ||
                                   Object.getOwnPropertyDescriptor(Location.prototype, 'href');

      // Override window.location.href to prevent redirects
      Object.defineProperty(window.location, 'href', {
        set: function(url) {
          console.log('🚫 REDIRECT PREVENTED:', url);
          console.log('🚫 Redirect prevention is enabled in Auth Debugger');
        },
        get: function() {
          return window.location.toString();
        }
      });

      return () => {
        // Restore original behavior
        if (originalLocationHref) {
          Object.defineProperty(window.location, 'href', originalLocationHref);
        }
      };
    }
  }, [preventRedirects]);

  // Capture console logs
  useEffect(() => {
    const originalLog = console.log;
    const originalError = console.error;
    const originalWarn = console.warn;

    const captureLog = (level: string, ...args: any[]) => {
      const timestamp = new Date().toISOString();
      const message = `[${timestamp}] ${level}: ${args.map(arg =>
        typeof arg === 'object' ? JSON.stringify(arg, null, 2) : String(arg)
      ).join(' ')}`;

      setConsoleLogs(prev => [...prev.slice(-50), message]); // Keep last 50 logs

      // Call original function
      if (level === 'ERROR') originalError(...args);
      else if (level === 'WARN') originalWarn(...args);
      else originalLog(...args);
    };

    console.log = (...args) => captureLog('LOG', ...args);
    console.error = (...args) => captureLog('ERROR', ...args);
    console.warn = (...args) => captureLog('WARN', ...args);

    return () => {
      console.log = originalLog;
      console.error = originalError;
      console.warn = originalWarn;
    };
  }, []);

  const collectDebugInfo = async () => {
    try {
      // Get localStorage auth data
      const authStorage = localStorage.getItem('auth-storage');
      let parsedAuthStorage = null;
      let decryptedToken = null;

      if (authStorage) {
        try {
          parsedAuthStorage = JSON.parse(authStorage);
          if (parsedAuthStorage?.state?.token) {
            try {
              decryptedToken = await SecurityUtils.decryptToken(parsedAuthStorage.state.token);
            } catch (decryptError) {
              console.error('Token decryption failed:', decryptError);
            }
          }
        } catch (parseError) {
          console.error('Auth storage parse error:', parseError);
        }
      }

      // Check session storage
      const sessionKeys = Object.keys(sessionStorage);
      const sessionData: any = {};
      sessionKeys.forEach(key => {
        sessionData[key] = sessionStorage.getItem(key);
      });

      // Check localStorage for other auth-related items
      const localStorageKeys = Object.keys(localStorage).filter(key => 
        key.includes('auth') || key.includes('token') || key.includes('user')
      );
      const localStorageData: any = {};
      localStorageKeys.forEach(key => {
        localStorageData[key] = localStorage.getItem(key);
      });

      setDebugInfo({
        authState: {
          isAuthenticated: authState.isAuthenticated,
          hasUser: !!authState.user,
          hasToken: !!authState.token,
          hasRefreshToken: !!authState.refreshToken,
          isAdmin: authState.isAdmin,
          user: authState.user
        },
        localStorage: {
          authStorage: parsedAuthStorage,
          otherAuthItems: localStorageData
        },
        sessionStorage: sessionData,
        decryptedToken: decryptedToken ? 'Token exists (hidden for security)' : 'No token or decryption failed',
        timestamp: new Date().toISOString()
      });
    } catch (error) {
      console.error('Debug info collection failed:', error);
      setDebugInfo({ error: error.message });
    }
  };

  useEffect(() => {
    collectDebugInfo();
  }, [authState]);

  const clearAllAuth = () => {
    // Clear Zustand store
    authState.logout();

    // Clear localStorage
    Object.keys(localStorage).forEach(key => {
      if (key.includes('auth') || key.includes('token') || key.includes('user')) {
        localStorage.removeItem(key);
      }
    });

    // Clear sessionStorage
    sessionStorage.clear();

    console.log('🧹 All authentication data cleared');
    collectDebugInfo();
  };

  const clearLogs = () => {
    setConsoleLogs([]);
  };

  const testLogin = async () => {
    console.log('🧪 Testing login with demo credentials...');
    try {
      const success = await authState.login('demo', 'demo');
      console.log('🧪 Login result:', success);
      collectDebugInfo();
    } catch (error) {
      console.error('🧪 Login test failed:', error);
    }
  };

  return (
    <div style={{ padding: '20px', maxWidth: '1200px', margin: '0 auto' }}>
      <Title level={2}>🔍 Authentication Debugger</Title>
      
      <Space direction="vertical" style={{ width: '100%' }} size="large">
        <Alert
          message="Debug Mode Active"
          description="This component helps diagnose authentication issues. Check the console for detailed logs."
          type="info"
          showIcon
        />

        <Space wrap>
          <Button onClick={collectDebugInfo}>Refresh Debug Info</Button>
          <Button onClick={clearAllAuth} danger>Clear All Auth Data</Button>
          <Button onClick={testLogin} type="primary">Test Login</Button>
          <Button onClick={clearLogs}>Clear Console Logs</Button>
          <Button
            onClick={() => setPreventRedirects(!preventRedirects)}
            type={preventRedirects ? "primary" : "default"}
            style={{ backgroundColor: preventRedirects ? '#52c41a' : undefined }}
          >
            {preventRedirects ? '🚫 Redirects Blocked' : '🔄 Allow Redirects'}
          </Button>
        </Space>

        <Collapse
          items={[
            {
              key: 'console-logs',
              label: `Console Logs (${consoleLogs.length})`,
              children: (
                <div style={{
                  background: '#000',
                  color: '#00ff00',
                  padding: '10px',
                  borderRadius: '4px',
                  fontFamily: 'monospace',
                  fontSize: '12px',
                  maxHeight: '400px',
                  overflowY: 'auto'
                }}>
                  {consoleLogs.length === 0 ? (
                    <div style={{ color: '#888' }}>No logs captured yet...</div>
                  ) : (
                    consoleLogs.map((log, index) => (
                      <div key={index} style={{
                        marginBottom: '2px',
                        color: log.includes('ERROR') ? '#ff4444' :
                              log.includes('WARN') ? '#ffaa00' :
                              log.includes('🚨') ? '#ff6666' :
                              log.includes('🔐') ? '#44ff44' :
                              log.includes('🚪') ? '#ff8844' :
                              log.includes('🔒') ? '#ffff44' : '#00ff00'
                      }}>
                        {log}
                      </div>
                    ))
                  )}
                </div>
              )
            },
            {
              key: 'auth-state',
              label: 'Current Auth State',
              children: (
                <pre style={{ background: '#f5f5f5', padding: '10px', borderRadius: '4px' }}>
                  {JSON.stringify(debugInfo.authState, null, 2)}
                </pre>
              )
            },
            {
              key: 'localStorage',
              label: 'localStorage Data',
              children: (
                <pre style={{ background: '#f5f5f5', padding: '10px', borderRadius: '4px' }}>
                  {JSON.stringify(debugInfo.localStorage, null, 2)}
                </pre>
              )
            },
            {
              key: 'sessionStorage',
              label: 'sessionStorage Data',
              children: (
                <pre style={{ background: '#f5f5f5', padding: '10px', borderRadius: '4px' }}>
                  {JSON.stringify(debugInfo.sessionStorage, null, 2)}
                </pre>
              )
            },
            {
              key: 'full-debug',
              label: 'Full Debug Info',
              children: (
                <pre style={{ background: '#f5f5f5', padding: '10px', borderRadius: '4px' }}>
                  {JSON.stringify(debugInfo, null, 2)}
                </pre>
              )
            }
          ]}
          defaultActiveKey={['console-logs', 'auth-state']}
        />

        <Card title="Console Monitoring">
          <Text>
            Open your browser's Developer Tools (F12) and check the Console tab for detailed authentication logs.
            Look for messages starting with 🔐, 🔒, or ❌.
          </Text>
        </Card>
      </Space>
    </div>
  );
};

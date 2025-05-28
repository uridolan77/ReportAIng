// Connection Test Utility for BI Reporting Copilot
// This utility helps diagnose frontend-backend connection issues

import { API_BASE_URL, HEALTH_ENDPOINTS } from '../config/endpoints';

export interface ConnectionTestResult {
  success: boolean;
  message: string;
  details?: any;
  timestamp: string;
}

export class ConnectionTester {
  private static readonly API_BASE_URL = API_BASE_URL;

  /**
   * Test basic connectivity to the API server
   */
  static async testBasicConnection(): Promise<ConnectionTestResult> {
    const timestamp = new Date().toISOString();

    try {
      console.log(`Testing connection to: ${this.API_BASE_URL}`);

      const response = await fetch(`${this.API_BASE_URL}${HEALTH_ENDPOINTS.SYSTEM_HEALTH}`, {
        method: 'GET',
        headers: {
          'Content-Type': 'application/json',
        },
        // Add credentials for CORS
        credentials: 'include',
      });

      if (response.ok) {
        let data: any;
        const contentType = response.headers.get('content-type');

        try {
          if (contentType && contentType.includes('application/json')) {
            data = await response.json();
          } else {
            // Handle plain text response (like "Healthy", "Degraded", etc.)
            const textData = await response.text();
            data = { status: textData, raw: textData };
          }
        } catch (parseError) {
          // If JSON parsing fails, try to get as text
          const textData = await response.text();
          data = { status: textData, raw: textData, parseError: (parseError as Error).message };
        }

        return {
          success: true,
          message: `API server is responding (Status: ${data.status || data.raw || 'Unknown'})`,
          details: {
            status: response.status,
            statusText: response.statusText,
            contentType: contentType,
            data: data,
            url: `${this.API_BASE_URL}${HEALTH_ENDPOINTS.SYSTEM_HEALTH}`
          },
          timestamp
        };
      } else {
        let errorData: any;
        try {
          errorData = await response.json();
        } catch {
          errorData = await response.text();
        }

        return {
          success: false,
          message: `API server responded with error: ${response.status} ${response.statusText}`,
          details: {
            status: response.status,
            statusText: response.statusText,
            errorData: errorData,
            url: `${this.API_BASE_URL}${HEALTH_ENDPOINTS.SYSTEM_HEALTH}`
          },
          timestamp
        };
      }
    } catch (error: any) {
      return {
        success: false,
        message: `Failed to connect to API server: ${error.message}`,
        details: {
          error: error.message,
          stack: error.stack,
          url: `${this.API_BASE_URL}/health`,
          possibleCauses: [
            'API server is not running',
            'CORS configuration issue',
            'SSL certificate problem',
            'Network connectivity issue',
            'Firewall blocking the connection'
          ]
        },
        timestamp
      };
    }
  }

  /**
   * Test authentication endpoint
   */
  static async testAuthEndpoint(): Promise<ConnectionTestResult> {
    const timestamp = new Date().toISOString();

    try {
      const response = await fetch(`${this.API_BASE_URL}/api/auth/validate`, {
        method: 'GET',
        headers: {
          'Content-Type': 'application/json',
        },
        credentials: 'include',
      });

      // 401 is expected for unauthenticated requests
      if (response.status === 401) {
        return {
          success: true,
          message: 'Auth endpoint is accessible (401 expected for unauthenticated request)',
          details: {
            status: response.status,
            statusText: response.statusText,
            url: `${this.API_BASE_URL}/api/auth/validate`
          },
          timestamp
        };
      } else if (response.ok) {
        const data = await response.json();
        return {
          success: true,
          message: 'Auth endpoint responded successfully',
          details: {
            status: response.status,
            statusText: response.statusText,
            data: data,
            url: `${this.API_BASE_URL}/api/auth/validate`
          },
          timestamp
        };
      } else {
        return {
          success: false,
          message: `Auth endpoint error: ${response.status} ${response.statusText}`,
          details: {
            status: response.status,
            statusText: response.statusText,
            url: `${this.API_BASE_URL}/api/auth/validate`
          },
          timestamp
        };
      }
    } catch (error: any) {
      return {
        success: false,
        message: `Failed to reach auth endpoint: ${error.message}`,
        details: {
          error: error.message,
          url: `${this.API_BASE_URL}/api/auth/validate`
        },
        timestamp
      };
    }
  }

  /**
   * Test detailed health endpoint (should return JSON)
   */
  static async testDetailedHealth(): Promise<ConnectionTestResult> {
    const timestamp = new Date().toISOString();

    try {
      const response = await fetch(`${this.API_BASE_URL}/api/health/detailed`, {
        method: 'GET',
        headers: {
          'Content-Type': 'application/json',
        },
        credentials: 'include',
      });

      if (response.ok) {
        const data = await response.json();
        return {
          success: true,
          message: 'Detailed health endpoint responded successfully',
          details: {
            status: response.status,
            statusText: response.statusText,
            data: data,
            url: `${this.API_BASE_URL}/api/health/detailed`
          },
          timestamp
        };
      } else {
        return {
          success: false,
          message: `Detailed health endpoint error: ${response.status} ${response.statusText}`,
          details: {
            status: response.status,
            statusText: response.statusText,
            url: `${this.API_BASE_URL}/api/health/detailed`
          },
          timestamp
        };
      }
    } catch (error: any) {
      return {
        success: false,
        message: `Failed to reach detailed health endpoint: ${error.message}`,
        details: {
          error: error.message,
          url: `${this.API_BASE_URL}/api/health/detailed`
        },
        timestamp
      };
    }
  }

  /**
   * Run comprehensive connection tests
   */
  static async runAllTests(): Promise<ConnectionTestResult[]> {
    console.log('Running comprehensive connection tests...');

    const results: ConnectionTestResult[] = [];

    // Test basic health endpoint
    console.log('1. Testing health endpoint...');
    results.push(await this.testBasicConnection());

    // Test detailed health endpoint
    console.log('2. Testing detailed health endpoint...');
    results.push(await this.testDetailedHealth());

    // Test auth endpoint
    console.log('3. Testing auth endpoint...');
    results.push(await this.testAuthEndpoint());

    return results;
  }

  /**
   * Display test results in console
   */
  static displayResults(results: ConnectionTestResult[]): void {
    console.log('\n=== Connection Test Results ===');

    results.forEach((result, index) => {
      console.log(`\nTest ${index + 1}:`);
      console.log(`Status: ${result.success ? '✅ PASS' : '❌ FAIL'}`);
      console.log(`Message: ${result.message}`);
      console.log(`Timestamp: ${result.timestamp}`);

      if (result.details) {
        console.log('Details:', result.details);
      }
    });

    const passCount = results.filter(r => r.success).length;
    const totalCount = results.length;

    console.log(`\n=== Summary ===`);
    console.log(`Passed: ${passCount}/${totalCount}`);

    if (passCount === totalCount) {
      console.log('🎉 All tests passed! Frontend should be able to connect to the API.');
    } else {
      console.log('⚠️ Some tests failed. Check the details above for troubleshooting.');
    }
  }
}

// Export for easy use in browser console
(window as any).ConnectionTester = ConnectionTester;

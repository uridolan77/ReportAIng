import { test, expect } from '@playwright/test';

test.describe('Visual Regression Tests', () => {
  test.beforeEach(async ({ page }) => {
    // Login first
    await page.goto('/');
    await page.fill('input[placeholder*="username" i]', 'testuser');
    await page.fill('input[type="password"]', 'password');
    await page.click('button[type="submit"]');
    await expect(page.locator('text=Query Interface')).toBeVisible();
  });

  test('should match login page screenshot', async ({ page }) => {
    await page.goto('/login');
    
    // Wait for page to be fully loaded
    await page.waitForLoadState('networkidle');
    
    // Take screenshot and compare
    await expect(page).toHaveScreenshot('login-page.png');
  });

  test('should match main dashboard screenshot', async ({ page }) => {
    await page.goto('/');
    
    // Wait for all content to load
    await page.waitForLoadState('networkidle');
    await page.waitForTimeout(2000); // Allow for any animations
    
    // Take screenshot
    await expect(page).toHaveScreenshot('main-dashboard.png');
  });

  test('should match advanced demo page screenshot', async ({ page }) => {
    await page.goto('/advanced-demo');
    
    // Wait for charts to load
    await page.waitForSelector('svg', { timeout: 10000 });
    await page.waitForLoadState('networkidle');
    await page.waitForTimeout(3000); // Allow for chart animations
    
    // Take screenshot
    await expect(page).toHaveScreenshot('advanced-demo.png');
  });

  test('should match heatmap chart screenshot', async ({ page }) => {
    await page.goto('/advanced-demo');
    
    // Wait for heatmap to load
    await page.waitForSelector('svg', { timeout: 10000 });
    await page.waitForTimeout(2000);
    
    // Take screenshot of specific chart
    const heatmapCard = page.locator('text=Interactive Heatmap').locator('..').locator('..');
    await expect(heatmapCard).toHaveScreenshot('heatmap-chart.png');
  });

  test('should match treemap chart screenshot', async ({ page }) => {
    await page.goto('/advanced-demo');
    
    // Wait for treemap to load
    await page.waitForSelector('svg', { timeout: 10000 });
    await page.waitForTimeout(2000);
    
    // Take screenshot of treemap
    const treemapCard = page.locator('text=Hierarchical Treemap').locator('..').locator('..');
    await expect(treemapCard).toHaveScreenshot('treemap-chart.png');
  });

  test('should match network chart screenshot', async ({ page }) => {
    await page.goto('/advanced-demo');
    
    // Wait for network chart to load and stabilize
    await page.waitForSelector('svg', { timeout: 10000 });
    await page.waitForTimeout(5000); // Network charts need more time to stabilize
    
    // Take screenshot of network chart
    const networkCard = page.locator('text=Network Visualization').locator('..').locator('..');
    await expect(networkCard).toHaveScreenshot('network-chart.png');
  });

  test('should match performance tab screenshot', async ({ page }) => {
    await page.goto('/advanced-demo');
    
    // Navigate to performance tab
    await page.click('text=Performance Features');
    await page.waitForLoadState('networkidle');
    await page.waitForTimeout(1000);
    
    // Take screenshot
    await expect(page).toHaveScreenshot('performance-tab.png');
  });

  test('should match state management tab screenshot', async ({ page }) => {
    await page.goto('/advanced-demo');
    
    // Navigate to state management tab
    await page.click('text=State Management');
    await page.waitForLoadState('networkidle');
    
    // Take screenshot
    await expect(page).toHaveScreenshot('state-management-tab.png');
  });

  test('should match mobile layout screenshot', async ({ page }) => {
    // Set mobile viewport
    await page.setViewportSize({ width: 375, height: 667 });
    
    await page.goto('/advanced-demo');
    await page.waitForLoadState('networkidle');
    await page.waitForTimeout(2000);
    
    // Take mobile screenshot
    await expect(page).toHaveScreenshot('mobile-layout.png');
  });

  test('should match tablet layout screenshot', async ({ page }) => {
    // Set tablet viewport
    await page.setViewportSize({ width: 768, height: 1024 });
    
    await page.goto('/advanced-demo');
    await page.waitForLoadState('networkidle');
    await page.waitForTimeout(2000);
    
    // Take tablet screenshot
    await expect(page).toHaveScreenshot('tablet-layout.png');
  });

  test('should match dark theme screenshot', async ({ page }) => {
    await page.goto('/advanced-demo');
    
    // Switch to dark theme if available
    const themeSelector = page.locator('select').filter({ hasText: 'Light' });
    if (await themeSelector.isVisible()) {
      await themeSelector.selectOption('dark');
      await page.waitForTimeout(1000);
    }
    
    // Take screenshot
    await expect(page).toHaveScreenshot('dark-theme.png');
  });

  test('should match error state screenshot', async ({ page }) => {
    // Navigate to a route that triggers error boundary
    await page.goto('/non-existent-route');
    await page.waitForTimeout(1000);
    
    // Take screenshot of error state
    await expect(page).toHaveScreenshot('error-state.png');
  });

  test('should match loading state screenshot', async ({ page }) => {
    // Intercept network requests to simulate slow loading
    await page.route('**/*', route => {
      setTimeout(() => route.continue(), 2000);
    });
    
    const pagePromise = page.goto('/advanced-demo');
    
    // Take screenshot during loading
    await page.waitForTimeout(500);
    await expect(page).toHaveScreenshot('loading-state.png');
    
    await pagePromise;
  });
});

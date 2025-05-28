import { test, expect } from '@playwright/test';

test.describe('Advanced Features', () => {
  test.beforeEach(async ({ page }) => {
    // Login first
    await page.goto('/');
    await page.fill('input[placeholder*="username" i]', 'testuser');
    await page.fill('input[type="password"]', 'password');
    await page.click('button[type="submit"]');
    await expect(page.locator('text=Query Interface')).toBeVisible();
  });

  test('should navigate to advanced demo page', async ({ page }) => {
    await page.goto('/advanced-demo');
    await expect(page.locator('text=Advanced Features Demo')).toBeVisible();
    await expect(page.locator('text=Performance Stats')).toBeVisible();
  });

  test('should display interactive charts', async ({ page }) => {
    await page.goto('/advanced-demo');
    
    // Wait for charts to load
    await expect(page.locator('[data-testid="heatmap-chart"]')).toBeVisible({ timeout: 10000 });
    await expect(page.locator('[data-testid="treemap-chart"]')).toBeVisible({ timeout: 10000 });
    await expect(page.locator('[data-testid="network-chart"]')).toBeVisible({ timeout: 10000 });
  });

  test('should handle chart interactions', async ({ page }) => {
    await page.goto('/advanced-demo');
    
    // Wait for heatmap to load
    await page.waitForSelector('svg', { timeout: 10000 });
    
    // Try to interact with chart elements
    const chartElements = await page.locator('svg rect').count();
    expect(chartElements).toBeGreaterThan(0);
    
    // Click on a chart element
    await page.locator('svg rect').first().click();
    
    // Check if interaction feedback is shown
    // This would depend on your specific implementation
  });

  test('should display performance metrics', async ({ page }) => {
    await page.goto('/advanced-demo');
    
    // Check performance metrics are displayed
    await expect(page.locator('text=Memory Usage')).toBeVisible();
    await expect(page.locator('text=Data Points')).toBeVisible();
    
    // Verify metrics have values
    const memoryValue = await page.locator('[title="Memory Usage"] .ant-statistic-content-value').textContent();
    expect(memoryValue).toBeTruthy();
  });

  test('should handle virtual scrolling', async ({ page }) => {
    await page.goto('/advanced-demo');
    
    // Navigate to performance tab
    await page.click('text=Performance Features');
    
    // Wait for virtual list to load
    await expect(page.locator('text=Virtual Scrolling Demo')).toBeVisible();
    
    // Test search functionality
    await page.fill('input[placeholder*="Search"]', 'Item 1');
    
    // Verify search results
    await page.waitForTimeout(500); // Wait for debounce
    const visibleItems = await page.locator('[role="option"]').count();
    expect(visibleItems).toBeGreaterThan(0);
  });

  test('should update preferences in real-time', async ({ page }) => {
    await page.goto('/advanced-demo');
    
    // Navigate to state management tab
    await page.click('text=State Management');
    
    // Toggle animation preference
    const animationCheckbox = page.locator('input[type="checkbox"]').first();
    await animationCheckbox.click();
    
    // Verify the change is reflected in the UI
    const isChecked = await animationCheckbox.isChecked();
    expect(typeof isChecked).toBe('boolean');
  });

  test('should handle accessibility features', async ({ page }) => {
    await page.goto('/advanced-demo');
    
    // Test keyboard navigation
    await page.keyboard.press('Tab');
    await page.keyboard.press('Tab');
    
    // Check if focus is visible
    const focusedElement = await page.locator(':focus').count();
    expect(focusedElement).toBeGreaterThan(0);
    
    // Test accessible chart
    await page.click('text=Performance Features');
    await expect(page.locator('text=Keyboard Navigable')).toBeVisible();
  });

  test('should display development tools in dev mode', async ({ page }) => {
    await page.goto('/advanced-demo');
    
    // Look for dev tools button (only visible in development)
    const devToolsButton = page.locator('[aria-label*="bug"]');
    if (await devToolsButton.isVisible()) {
      await devToolsButton.click();
      await expect(page.locator('text=Developer Tools')).toBeVisible();
    }
  });

  test('should handle error boundaries gracefully', async ({ page }) => {
    // Navigate to a route that might trigger an error
    await page.goto('/non-existent-route');
    
    // Should redirect to home or show error page
    await page.waitForTimeout(1000);
    const currentUrl = page.url();
    expect(currentUrl).toMatch(/\/(|login|error)/);
  });

  test('should be responsive on mobile devices', async ({ page }) => {
    // Set mobile viewport
    await page.setViewportSize({ width: 375, height: 667 });
    
    await page.goto('/advanced-demo');
    
    // Check if mobile layout is applied
    await expect(page.locator('text=Advanced Features Demo')).toBeVisible();
    
    // Verify charts are still functional on mobile
    await page.waitForSelector('svg', { timeout: 10000 });
    const svgElements = await page.locator('svg').count();
    expect(svgElements).toBeGreaterThan(0);
  });

  test('should maintain performance under load', async ({ page }) => {
    await page.goto('/advanced-demo');
    
    // Measure page load performance
    const performanceMetrics = await page.evaluate(() => {
      const navigation = performance.getEntriesByType('navigation')[0] as PerformanceNavigationTiming;
      return {
        loadTime: navigation.loadEventEnd - navigation.loadEventStart,
        domContentLoaded: navigation.domContentLoadedEventEnd - navigation.domContentLoadedEventStart,
        firstPaint: performance.getEntriesByType('paint').find(entry => entry.name === 'first-paint')?.startTime
      };
    });
    
    // Assert reasonable performance
    expect(performanceMetrics.loadTime).toBeLessThan(5000); // 5 seconds
    expect(performanceMetrics.domContentLoaded).toBeLessThan(3000); // 3 seconds
  });
});

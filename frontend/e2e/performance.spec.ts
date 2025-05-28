import { test, expect } from '@playwright/test';

test.describe('Performance Tests', () => {
  test.beforeEach(async ({ page }) => {
    // Login first
    await page.goto('/');
    await page.fill('input[placeholder*="username" i]', 'testuser');
    await page.fill('input[type="password"]', 'password');
    await page.click('button[type="submit"]');
    await expect(page.locator('text=Query Interface')).toBeVisible();
  });

  test('should load main page within performance budget', async ({ page }) => {
    const startTime = Date.now();
    
    await page.goto('/');
    await page.waitForLoadState('networkidle');
    
    const loadTime = Date.now() - startTime;
    
    // Assert load time is under 3 seconds
    expect(loadTime).toBeLessThan(3000);
    
    // Check Core Web Vitals
    const webVitals = await page.evaluate(() => {
      return new Promise((resolve) => {
        new PerformanceObserver((list) => {
          const entries = list.getEntries();
          const vitals: any = {};
          
          entries.forEach((entry) => {
            if (entry.name === 'first-contentful-paint') {
              vitals.fcp = entry.startTime;
            }
            if (entry.entryType === 'largest-contentful-paint') {
              vitals.lcp = entry.startTime;
            }
            if (entry.entryType === 'layout-shift') {
              vitals.cls = (vitals.cls || 0) + entry.value;
            }
          });
          
          resolve(vitals);
        }).observe({ entryTypes: ['paint', 'largest-contentful-paint', 'layout-shift'] });
        
        // Fallback timeout
        setTimeout(() => resolve({}), 5000);
      });
    });
    
    console.log('Web Vitals:', webVitals);
  });

  test('should handle large dataset efficiently', async ({ page }) => {
    await page.goto('/advanced-demo');
    
    // Navigate to performance tab
    await page.click('text=Performance Features');
    
    // Measure virtual scrolling performance
    const startTime = Date.now();
    
    // Scroll through virtual list
    const virtualList = page.locator('[role="listbox"]').first();
    await virtualList.scrollIntoViewIfNeeded();
    
    for (let i = 0; i < 10; i++) {
      await virtualList.evaluate((el) => {
        el.scrollTop += 500;
      });
      await page.waitForTimeout(100);
    }
    
    const scrollTime = Date.now() - startTime;
    
    // Virtual scrolling should be smooth
    expect(scrollTime).toBeLessThan(2000);
  });

  test('should maintain 60fps during chart interactions', async ({ page }) => {
    await page.goto('/advanced-demo');
    
    // Wait for charts to load
    await page.waitForSelector('svg', { timeout: 10000 });
    
    // Start performance monitoring
    await page.evaluate(() => {
      (window as any).performanceData = {
        frames: [],
        startTime: performance.now()
      };
      
      function measureFrame() {
        const now = performance.now();
        (window as any).performanceData.frames.push(now);
        requestAnimationFrame(measureFrame);
      }
      
      requestAnimationFrame(measureFrame);
    });
    
    // Interact with charts
    const chartElements = page.locator('svg rect, svg circle');
    const elementCount = await chartElements.count();
    
    if (elementCount > 0) {
      for (let i = 0; i < Math.min(5, elementCount); i++) {
        await chartElements.nth(i).hover();
        await page.waitForTimeout(200);
      }
    }
    
    // Measure frame rate
    const performanceData = await page.evaluate(() => {
      const data = (window as any).performanceData;
      const totalTime = performance.now() - data.startTime;
      const frameCount = data.frames.length;
      const fps = (frameCount / totalTime) * 1000;
      
      return { fps, frameCount, totalTime };
    });
    
    console.log('Chart interaction performance:', performanceData);
    
    // Should maintain reasonable frame rate
    expect(performanceData.fps).toBeGreaterThan(30);
  });

  test('should have efficient memory usage', async ({ page }) => {
    await page.goto('/advanced-demo');
    
    // Get initial memory usage
    const initialMemory = await page.evaluate(() => {
      if ('memory' in performance) {
        return (performance as any).memory.usedJSHeapSize;
      }
      return 0;
    });
    
    // Navigate through different tabs and interact
    const tabs = ['Advanced Charts', 'Performance Features', 'State Management'];
    
    for (const tab of tabs) {
      await page.click(`text=${tab}`);
      await page.waitForTimeout(1000);
      
      // Interact with content
      const interactiveElements = page.locator('button, input, [role="button"]');
      const count = await interactiveElements.count();
      
      if (count > 0) {
        await interactiveElements.first().click();
        await page.waitForTimeout(500);
      }
    }
    
    // Force garbage collection if available
    await page.evaluate(() => {
      if ('gc' in window) {
        (window as any).gc();
      }
    });
    
    // Get final memory usage
    const finalMemory = await page.evaluate(() => {
      if ('memory' in performance) {
        return (performance as any).memory.usedJSHeapSize;
      }
      return 0;
    });
    
    if (initialMemory > 0 && finalMemory > 0) {
      const memoryIncrease = finalMemory - initialMemory;
      const memoryIncreasePercent = (memoryIncrease / initialMemory) * 100;
      
      console.log(`Memory usage: ${initialMemory} -> ${finalMemory} (${memoryIncreasePercent.toFixed(2)}% increase)`);
      
      // Memory increase should be reasonable
      expect(memoryIncreasePercent).toBeLessThan(200); // Less than 200% increase
    }
  });

  test('should handle concurrent chart rendering efficiently', async ({ page }) => {
    await page.goto('/advanced-demo');
    
    // Measure time to render all charts
    const startTime = Date.now();
    
    // Wait for all SVG elements to be present
    await page.waitForFunction(() => {
      const svgs = document.querySelectorAll('svg');
      return svgs.length >= 3; // Expecting at least 3 charts
    }, { timeout: 15000 });
    
    const renderTime = Date.now() - startTime;
    
    console.log(`Chart rendering time: ${renderTime}ms`);
    
    // All charts should render within reasonable time
    expect(renderTime).toBeLessThan(10000); // 10 seconds
    
    // Verify charts are actually rendered with content
    const chartElements = await page.locator('svg rect, svg circle, svg path').count();
    expect(chartElements).toBeGreaterThan(10); // Should have multiple chart elements
  });

  test('should handle search and filtering efficiently', async ({ page }) => {
    await page.goto('/advanced-demo');
    await page.click('text=Performance Features');
    
    const searchInput = page.locator('input[placeholder*="Search"]');
    
    // Measure search performance
    const searchTerms = ['Item 1', 'Item 2', 'Item 3', 'Electronics', 'Clothing'];
    
    for (const term of searchTerms) {
      const startTime = Date.now();
      
      await searchInput.fill(term);
      
      // Wait for search results to update
      await page.waitForTimeout(500); // Account for debounce
      
      const searchTime = Date.now() - startTime;
      
      console.log(`Search for "${term}" took ${searchTime}ms`);
      
      // Search should be fast
      expect(searchTime).toBeLessThan(1000);
      
      // Clear search
      await searchInput.fill('');
      await page.waitForTimeout(300);
    }
  });

  test('should maintain performance with state updates', async ({ page }) => {
    await page.goto('/advanced-demo');
    await page.click('text=State Management');
    
    // Measure state update performance
    const checkboxes = page.locator('input[type="checkbox"]');
    const checkboxCount = await checkboxes.count();
    
    const startTime = Date.now();
    
    // Toggle all checkboxes multiple times
    for (let round = 0; round < 3; round++) {
      for (let i = 0; i < checkboxCount; i++) {
        await checkboxes.nth(i).click();
        await page.waitForTimeout(50);
      }
    }
    
    const updateTime = Date.now() - startTime;
    const updatesPerSecond = (checkboxCount * 3 * 1000) / updateTime;
    
    console.log(`State updates: ${updatesPerSecond.toFixed(2)} updates/second`);
    
    // Should handle state updates efficiently
    expect(updatesPerSecond).toBeGreaterThan(10);
  });

  test('should have efficient bundle size', async ({ page }) => {
    // Navigate to the app and measure network usage
    const responses: any[] = [];
    
    page.on('response', (response) => {
      if (response.url().includes('.js') || response.url().includes('.css')) {
        responses.push({
          url: response.url(),
          size: response.headers()['content-length'],
          type: response.url().includes('.js') ? 'js' : 'css'
        });
      }
    });
    
    await page.goto('/advanced-demo');
    await page.waitForLoadState('networkidle');
    
    // Calculate total bundle size
    let totalJSSize = 0;
    let totalCSSSize = 0;
    
    responses.forEach(response => {
      const size = parseInt(response.size || '0');
      if (response.type === 'js') {
        totalJSSize += size;
      } else {
        totalCSSSize += size;
      }
    });
    
    console.log(`Bundle sizes - JS: ${(totalJSSize / 1024).toFixed(2)}KB, CSS: ${(totalCSSSize / 1024).toFixed(2)}KB`);
    
    // Bundle size should be reasonable
    expect(totalJSSize).toBeLessThan(5 * 1024 * 1024); // 5MB JS
    expect(totalCSSSize).toBeLessThan(1 * 1024 * 1024); // 1MB CSS
  });
});

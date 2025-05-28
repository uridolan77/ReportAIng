import { test, expect } from '@playwright/test';

test.describe('Authentication Flow', () => {
  test.beforeEach(async ({ page }) => {
    await page.goto('/');
  });

  test('should show login page when not authenticated', async ({ page }) => {
    await expect(page.locator('text=Sign In')).toBeVisible();
    await expect(page.locator('input[placeholder*="username" i]')).toBeVisible();
    await expect(page.locator('input[type="password"]')).toBeVisible();
  });

  test('should login successfully with valid credentials', async ({ page }) => {
    // Fill login form
    await page.fill('input[placeholder*="username" i]', 'testuser');
    await page.fill('input[type="password"]', 'password');
    
    // Submit form
    await page.click('button[type="submit"]');
    
    // Wait for redirect to main app
    await expect(page).toHaveURL(/\/(?!login)/);
    
    // Verify main navigation is visible
    await expect(page.locator('text=Query Interface')).toBeVisible();
  });

  test('should show error message with invalid credentials', async ({ page }) => {
    // Fill login form with invalid credentials
    await page.fill('input[placeholder*="username" i]', 'wronguser');
    await page.fill('input[type="password"]', 'wrongpassword');
    
    // Submit form
    await page.click('button[type="submit"]');
    
    // Wait for error message
    await expect(page.locator('text=Login failed')).toBeVisible();
    
    // Should still be on login page
    await expect(page).toHaveURL(/\/login/);
  });

  test('should logout successfully', async ({ page }) => {
    // Login first
    await page.fill('input[placeholder*="username" i]', 'testuser');
    await page.fill('input[type="password"]', 'password');
    await page.click('button[type="submit"]');
    
    // Wait for main app
    await expect(page.locator('text=Query Interface')).toBeVisible();
    
    // Find and click logout button
    await page.click('text=Logout');
    
    // Should redirect to login page
    await expect(page).toHaveURL(/\/login/);
    await expect(page.locator('text=Sign In')).toBeVisible();
  });

  test('should persist authentication on page refresh', async ({ page }) => {
    // Login
    await page.fill('input[placeholder*="username" i]', 'testuser');
    await page.fill('input[type="password"]', 'password');
    await page.click('button[type="submit"]');
    
    // Wait for main app
    await expect(page.locator('text=Query Interface')).toBeVisible();
    
    // Refresh page
    await page.reload();
    
    // Should still be authenticated
    await expect(page.locator('text=Query Interface')).toBeVisible();
    await expect(page).toHaveURL(/\/(?!login)/);
  });

  test('should handle session expiry gracefully', async ({ page }) => {
    // Login
    await page.fill('input[placeholder*="username" i]', 'testuser');
    await page.fill('input[type="password"]', 'password');
    await page.click('button[type="submit"]');
    
    // Wait for main app
    await expect(page.locator('text=Query Interface')).toBeVisible();
    
    // Simulate session expiry by clearing storage
    await page.evaluate(() => {
      localStorage.clear();
      sessionStorage.clear();
    });
    
    // Try to navigate or perform an action
    await page.reload();
    
    // Should redirect to login
    await expect(page).toHaveURL(/\/login/);
  });
});

/**
 * Component Testing Utilities
 * Specialized utilities for testing different types of components
 */

import React from 'react';
import { screen, fireEvent, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { renderWithProviders } from './testing-providers';
import type { QueryTemplate, QueryShortcut } from '../services/queryTemplateService';

// Query Interface Testing Utilities
export class QueryInterfaceTestUtils {
  static async typeQuery(query: string) {
    const user = userEvent.setup();
    const input = screen.getByRole('textbox', { name: /query/i });
    await user.clear(input);
    await user.type(input, query);
    return input;
  }

  static async submitQuery() {
    const user = userEvent.setup();
    const submitButton = screen.getByRole('button', { name: /ask|submit/i });
    await user.click(submitButton);
    return submitButton;
  }

  static async selectSuggestion(suggestionText: string) {
    const user = userEvent.setup();
    const suggestion = screen.getByText(suggestionText);
    await user.click(suggestion);
    return suggestion;
  }

  static async openShortcutsPanel() {
    const user = userEvent.setup();
    const shortcutsButton = screen.getByRole('button', { name: /shortcuts/i });
    await user.click(shortcutsButton);
    return shortcutsButton;
  }

  static async selectTemplate(templateName: string) {
    const user = userEvent.setup();
    const template = screen.getByText(templateName);
    await user.click(template);
    return template;
  }

  static async fillTemplateVariable(variableName: string, value: string) {
    const user = userEvent.setup();
    const input = screen.getByLabelText(new RegExp(variableName, 'i'));
    await user.clear(input);
    await user.type(input, value);
    return input;
  }

  static async applyTemplate() {
    const user = userEvent.setup();
    const applyButton = screen.getByRole('button', { name: /apply template/i });
    await user.click(applyButton);
    return applyButton;
  }

  static getQueryInput() {
    return screen.getByRole('textbox', { name: /query/i });
  }

  static getSubmitButton() {
    return screen.getByRole('button', { name: /ask|submit/i });
  }

  static getLoadingIndicator() {
    return screen.queryByText(/processing/i);
  }

  static getErrorMessage() {
    return screen.queryByRole('alert');
  }

  static getResults() {
    return screen.queryByTestId('query-results');
  }
}

// Template Testing Utilities
export class TemplateTestUtils {
  static renderTemplate(template: QueryTemplate, props = {}) {
    const defaultProps = {
      template,
      onSelect: jest.fn(),
      onFavorite: jest.fn(),
      ...props,
    };

    return renderWithProviders(
      <div data-testid="template-item">
        {template.name}
      </div>
    );
  }

  static async toggleFavorite(templateName: string) {
    const user = userEvent.setup();
    const favoriteButton = screen.getByRole('button', { name: new RegExp(`favorite.*${templateName}`, 'i') });
    await user.click(favoriteButton);
    return favoriteButton;
  }

  static async filterByCategory(category: string) {
    const user = userEvent.setup();
    const categoryFilter = screen.getByRole('combobox', { name: /category/i });
    await user.click(categoryFilter);
    const option = screen.getByText(category);
    await user.click(option);
    return categoryFilter;
  }

  static async searchTemplates(searchTerm: string) {
    const user = userEvent.setup();
    const searchInput = screen.getByRole('textbox', { name: /search/i });
    await user.clear(searchInput);
    await user.type(searchInput, searchTerm);
    return searchInput;
  }

  static getTemplateCard(templateName: string) {
    return screen.getByText(templateName).closest('[data-testid="template-card"]');
  }

  static getTemplateVariables() {
    return screen.getAllByRole('textbox').filter(input => 
      input.getAttribute('name')?.includes('variable')
    );
  }
}

// Shortcut Testing Utilities
export class ShortcutTestUtils {
  static async useShortcut(shortcut: string) {
    const user = userEvent.setup();
    const input = screen.getByRole('textbox', { name: /query/i });
    await user.clear(input);
    await user.type(input, shortcut);
    
    // Wait for suggestions to appear
    await waitFor(() => {
      expect(screen.getByText(new RegExp(shortcut, 'i'))).toBeInTheDocument();
    });

    // Select the shortcut suggestion
    const suggestion = screen.getByText(new RegExp(shortcut, 'i'));
    await user.click(suggestion);
    
    return suggestion;
  }

  static async createShortcut(shortcutData: Partial<QueryShortcut>) {
    const user = userEvent.setup();
    
    // Open create shortcut modal
    const createButton = screen.getByRole('button', { name: /create.*shortcut/i });
    await user.click(createButton);

    // Fill form fields
    if (shortcutData.name) {
      const nameInput = screen.getByLabelText(/name/i);
      await user.type(nameInput, shortcutData.name);
    }

    if (shortcutData.shortcut) {
      const shortcutInput = screen.getByLabelText(/shortcut/i);
      await user.type(shortcutInput, shortcutData.shortcut);
    }

    if (shortcutData.query) {
      const queryInput = screen.getByLabelText(/query/i);
      await user.type(queryInput, shortcutData.query);
    }

    // Submit form
    const submitButton = screen.getByRole('button', { name: /create|save/i });
    await user.click(submitButton);

    return submitButton;
  }

  static getShortcutsList() {
    return screen.getByTestId('shortcuts-list');
  }

  static getShortcutItem(shortcut: string) {
    return screen.getByText(shortcut).closest('[data-testid="shortcut-item"]');
  }
}

// Visualization Testing Utilities
export class VisualizationTestUtils {
  static async selectVisualizationType(type: string) {
    const user = userEvent.setup();
    const typeSelector = screen.getByRole('combobox', { name: /visualization.*type/i });
    await user.click(typeSelector);
    const option = screen.getByText(type);
    await user.click(option);
    return typeSelector;
  }

  static async configureChart(config: Record<string, any>) {
    const user = userEvent.setup();
    
    for (const [key, value] of Object.entries(config)) {
      const input = screen.getByLabelText(new RegExp(key, 'i'));
      if (input.tagName === 'SELECT') {
        await user.selectOptions(input, value);
      } else {
        await user.clear(input);
        await user.type(input, value.toString());
      }
    }
  }

  static getChart() {
    return screen.getByTestId('chart-container');
  }

  static getChartLegend() {
    return screen.queryByTestId('chart-legend');
  }

  static getChartTooltip() {
    return screen.queryByTestId('chart-tooltip');
  }

  static async exportChart(format: string) {
    const user = userEvent.setup();
    const exportButton = screen.getByRole('button', { name: /export/i });
    await user.click(exportButton);
    const formatOption = screen.getByText(format);
    await user.click(formatOption);
    return exportButton;
  }
}

// Form Testing Utilities
export class FormTestUtils {
  static async fillForm(formData: Record<string, any>) {
    const user = userEvent.setup();
    
    for (const [fieldName, value] of Object.entries(formData)) {
      const field = screen.getByLabelText(new RegExp(fieldName, 'i'));
      
      if (field.tagName === 'SELECT') {
        await user.selectOptions(field, value);
      } else if (field.type === 'checkbox') {
        if (value && !field.checked) {
          await user.click(field);
        } else if (!value && field.checked) {
          await user.click(field);
        }
      } else if (field.type === 'radio') {
        if (value) {
          await user.click(field);
        }
      } else {
        await user.clear(field);
        await user.type(field, value.toString());
      }
    }
  }

  static async submitForm() {
    const user = userEvent.setup();
    const submitButton = screen.getByRole('button', { type: 'submit' });
    await user.click(submitButton);
    return submitButton;
  }

  static getFormErrors() {
    return screen.getAllByRole('alert').filter(alert => 
      alert.textContent?.includes('error') || alert.textContent?.includes('required')
    );
  }

  static getFieldError(fieldName: string) {
    const field = screen.getByLabelText(new RegExp(fieldName, 'i'));
    const fieldContainer = field.closest('.ant-form-item');
    return fieldContainer?.querySelector('.ant-form-item-explain-error');
  }
}

// Modal Testing Utilities
export class ModalTestUtils {
  static async openModal(triggerText: string) {
    const user = userEvent.setup();
    const trigger = screen.getByRole('button', { name: new RegExp(triggerText, 'i') });
    await user.click(trigger);
    
    await waitFor(() => {
      expect(screen.getByRole('dialog')).toBeInTheDocument();
    });
    
    return screen.getByRole('dialog');
  }

  static async closeModal() {
    const user = userEvent.setup();
    const closeButton = screen.getByRole('button', { name: /close|cancel/i });
    await user.click(closeButton);
    
    await waitFor(() => {
      expect(screen.queryByRole('dialog')).not.toBeInTheDocument();
    });
  }

  static async confirmModal() {
    const user = userEvent.setup();
    const confirmButton = screen.getByRole('button', { name: /ok|confirm|save|apply/i });
    await user.click(confirmButton);
    return confirmButton;
  }

  static getModal() {
    return screen.getByRole('dialog');
  }

  static getModalTitle() {
    return screen.getByRole('dialog').querySelector('.ant-modal-title');
  }

  static getModalContent() {
    return screen.getByRole('dialog').querySelector('.ant-modal-body');
  }
}

// Table Testing Utilities
export class TableTestUtils {
  static getTable() {
    return screen.getByRole('table');
  }

  static getTableRows() {
    return screen.getAllByRole('row').slice(1); // Exclude header row
  }

  static getTableHeaders() {
    return screen.getAllByRole('columnheader');
  }

  static async sortByColumn(columnName: string) {
    const user = userEvent.setup();
    const header = screen.getByRole('columnheader', { name: new RegExp(columnName, 'i') });
    await user.click(header);
    return header;
  }

  static async filterTable(filterValue: string) {
    const user = userEvent.setup();
    const filterInput = screen.getByRole('textbox', { name: /filter|search/i });
    await user.clear(filterInput);
    await user.type(filterInput, filterValue);
    return filterInput;
  }

  static async selectRow(rowIndex: number) {
    const user = userEvent.setup();
    const rows = this.getTableRows();
    const checkbox = rows[rowIndex].querySelector('input[type="checkbox"]');
    if (checkbox) {
      await user.click(checkbox);
    }
    return rows[rowIndex];
  }

  static getSelectedRows() {
    return this.getTableRows().filter(row => {
      const checkbox = row.querySelector('input[type="checkbox"]');
      return checkbox?.checked;
    });
  }
}

// Navigation Testing Utilities
export class NavigationTestUtils {
  static async navigateToPage(pageName: string) {
    const user = userEvent.setup();
    const link = screen.getByRole('link', { name: new RegExp(pageName, 'i') });
    await user.click(link);
    return link;
  }

  static async navigateToTab(tabName: string) {
    const user = userEvent.setup();
    const tab = screen.getByRole('tab', { name: new RegExp(tabName, 'i') });
    await user.click(tab);
    return tab;
  }

  static getCurrentPath() {
    return window.location.pathname;
  }

  static getActiveTab() {
    return screen.getByRole('tab', { selected: true });
  }

  static getBreadcrumbs() {
    return screen.getAllByRole('link').filter(link => 
      link.closest('.ant-breadcrumb')
    );
  }
}

// Accessibility Testing Utilities
export class AccessibilityTestUtils {
  static async navigateWithKeyboard(key: string, times: number = 1) {
    for (let i = 0; i < times; i++) {
      fireEvent.keyDown(document.activeElement || document.body, { key });
    }
  }

  static async tabToElement(targetElement: HTMLElement) {
    let currentElement = document.activeElement;
    let attempts = 0;
    const maxAttempts = 50;

    while (currentElement !== targetElement && attempts < maxAttempts) {
      fireEvent.keyDown(currentElement || document.body, { key: 'Tab' });
      currentElement = document.activeElement;
      attempts++;
    }

    return currentElement === targetElement;
  }

  static getFocusedElement() {
    return document.activeElement;
  }

  static getAriaLabel(element: HTMLElement) {
    return element.getAttribute('aria-label');
  }

  static getAriaDescribedBy(element: HTMLElement) {
    const describedBy = element.getAttribute('aria-describedby');
    if (!describedBy) return null;
    
    return describedBy.split(' ').map(id => document.getElementById(id)).filter(Boolean);
  }

  static hasAriaExpanded(element: HTMLElement) {
    return element.hasAttribute('aria-expanded');
  }

  static isAriaExpanded(element: HTMLElement) {
    return element.getAttribute('aria-expanded') === 'true';
  }
}

// Performance Testing Utilities
export class PerformanceTestUtils {
  static measureRenderTime<T>(renderFn: () => T): { result: T; renderTime: number } {
    const startTime = performance.now();
    const result = renderFn();
    const endTime = performance.now();
    
    return {
      result,
      renderTime: endTime - startTime,
    };
  }

  static async measureAsyncOperation<T>(operation: () => Promise<T>): Promise<{ result: T; duration: number }> {
    const startTime = performance.now();
    const result = await operation();
    const endTime = performance.now();
    
    return {
      result,
      duration: endTime - startTime,
    };
  }

  static createPerformanceObserver(entryTypes: string[] = ['measure', 'navigation']) {
    const entries: PerformanceEntry[] = [];
    
    const observer = new PerformanceObserver((list) => {
      entries.push(...list.getEntries());
    });
    
    observer.observe({ entryTypes });
    
    return {
      observer,
      getEntries: () => entries,
      disconnect: () => observer.disconnect(),
    };
  }
}

// All utilities are already exported above as individual exports

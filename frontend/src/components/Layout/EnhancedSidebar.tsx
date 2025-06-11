/**
 * Enhanced Sidebar Component
 * 
 * Complete sidebar navigation component with all available features including AI Tuning.
 */

import React, { useState } from 'react';
import { Link, useLocation } from 'react-router-dom';

export interface EnhancedSidebarProps {
  className?: string;
}

interface NavItem {
  path: string;
  label: string;
  icon?: string;
  category?: string;
  adminOnly?: boolean;
}

const EnhancedSidebar: React.FC<EnhancedSidebarProps> = ({ className = '' }) => {
  const location = useLocation();
  const [collapsedCategories, setCollapsedCategories] = useState<Set<string>>(new Set());

  // TODO: Get this from auth context
  const isAdmin = true; // For now, assume admin access

  const navItems: NavItem[] = [
    // Core Features
    { path: '/', label: '🏠 Home' },
    { path: '/results', label: '📊 Results' },
    { path: '/dashboard', label: '📈 Dashboard' },
    { path: '/visualization', label: '📉 Visualization' },
    
    // Query Tools
    { path: '/history', label: '📜 Query History', category: 'Query Tools' },
    { path: '/templates', label: '📝 Templates', category: 'Query Tools' },
    { path: '/suggestions', label: '💡 Suggestions', category: 'Query Tools' },
    
    // System Tools
    { path: '/db-explorer', label: '🗄️ DB Explorer', category: 'System Tools' },
    { path: '/performance', label: '⚡ Performance', category: 'System Tools' },
    
    // Admin Features
    { path: '/admin/tuning', label: '🤖 AI Tuning', category: 'Admin', adminOnly: true },
    { path: '/admin/schemas', label: '🗂️ Schema Management', category: 'Admin', adminOnly: true },
    { path: '/admin/cache', label: '💾 Cache Management', category: 'Admin', adminOnly: true },
    { path: '/admin/security', label: '🔒 Security', category: 'Admin', adminOnly: true },
    { path: '/admin/suggestions', label: '🎯 Suggestions Mgmt', category: 'Admin', adminOnly: true },
  ];

  const isActive = (path: string) => {
    if (path === '/') {
      return location.pathname === '/';
    }
    return location.pathname.startsWith(path);
  };

  const toggleCategory = (category: string) => {
    const newCollapsed = new Set(collapsedCategories);
    if (newCollapsed.has(category)) {
      newCollapsed.delete(category);
    } else {
      newCollapsed.add(category);
    }
    setCollapsedCategories(newCollapsed);
  };

  // Filter items based on admin access
  const filteredItems = navItems.filter(item => !item.adminOnly || isAdmin);

  // Group items by category
  const groupedItems = filteredItems.reduce((groups, item) => {
    const category = item.category || 'Main';
    if (!groups[category]) {
      groups[category] = [];
    }
    groups[category].push(item);
    return groups;
  }, {} as Record<string, NavItem[]>);

  const renderNavItem = (item: NavItem) => (
    <li key={item.path}>
      <Link
        to={item.path}
        style={{
          display: 'block',
          padding: '10px 20px',
          textDecoration: 'none',
          color: isActive(item.path) ? '#1890ff' : '#666',
          backgroundColor: isActive(item.path) ? '#e6f7ff' : 'transparent',
          borderRight: isActive(item.path) ? '3px solid #1890ff' : 'none',
          fontWeight: isActive(item.path) ? 600 : 400,
          fontSize: '14px',
          borderRadius: '4px',
          margin: '2px 8px',
          transition: 'all 0.2s ease',
        }}
        onMouseEnter={(e) => {
          if (!isActive(item.path)) {
            e.currentTarget.style.backgroundColor = '#f0f0f0';
          }
        }}
        onMouseLeave={(e) => {
          if (!isActive(item.path)) {
            e.currentTarget.style.backgroundColor = 'transparent';
          }
        }}
      >
        {item.label}
      </Link>
    </li>
  );

  const renderCategory = (categoryName: string, items: NavItem[]) => {
    const isCollapsed = collapsedCategories.has(categoryName);
    
    if (categoryName === 'Main') {
      return (
        <div key={categoryName} style={{ marginBottom: '20px' }}>
          <ul style={{ listStyle: 'none', margin: 0, padding: 0 }}>
            {items.map(renderNavItem)}
          </ul>
        </div>
      );
    }

    return (
      <div key={categoryName} style={{ marginBottom: '16px' }}>
        <div
          onClick={() => toggleCategory(categoryName)}
          style={{
            padding: '8px 20px',
            fontSize: '12px',
            fontWeight: 600,
            color: '#999',
            textTransform: 'uppercase',
            letterSpacing: '0.5px',
            cursor: 'pointer',
            display: 'flex',
            alignItems: 'center',
            justifyContent: 'space-between',
          }}
        >
          {categoryName}
          <span style={{ 
            transform: isCollapsed ? 'rotate(-90deg)' : 'rotate(0deg)',
            transition: 'transform 0.2s ease'
          }}>
            ▼
          </span>
        </div>
        {!isCollapsed && (
          <ul style={{ listStyle: 'none', margin: 0, padding: 0 }}>
            {items.map(renderNavItem)}
          </ul>
        )}
      </div>
    );
  };

  return (
    <div 
      className={`enhanced-sidebar ${className}`}
      style={{
        width: '280px',
        minHeight: '100vh',
        backgroundColor: '#fafafa',
        borderRight: '1px solid #e8e8e8',
        padding: '20px 0',
        overflowY: 'auto',
      }}
    >
      <div style={{ padding: '0 20px', marginBottom: '30px' }}>
        <h2 style={{ 
          margin: 0, 
          fontSize: '18px', 
          fontWeight: 600,
          color: '#1890ff'
        }}>
          BI Reporting Copilot
        </h2>
        <p style={{ 
          margin: '4px 0 0 0', 
          fontSize: '12px', 
          color: '#999',
          fontWeight: 400
        }}>
          AI-Powered Business Intelligence
        </p>
      </div>
      
      <nav>
        {Object.entries(groupedItems).map(([categoryName, items]) =>
          renderCategory(categoryName, items)
        )}
      </nav>

      {isAdmin && (
        <div style={{ 
          position: 'absolute', 
          bottom: '20px', 
          left: '20px', 
          right: '20px',
          padding: '12px',
          backgroundColor: '#e6f7ff',
          borderRadius: '6px',
          border: '1px solid #91d5ff'
        }}>
          <div style={{ fontSize: '12px', color: '#1890ff', fontWeight: 600 }}>
            👑 Admin Access
          </div>
          <div style={{ fontSize: '11px', color: '#666', marginTop: '2px' }}>
            Full system access enabled
          </div>
        </div>
      )}
    </div>
  );
};

export default EnhancedSidebar;

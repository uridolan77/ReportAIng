/**
 * Sidebar Component
 * 
 * Complete sidebar navigation component with all available features including AI Tuning.
 */

import React, { useState } from 'react';
import { Link, useLocation } from 'react-router-dom';

export interface SidebarProps {
  className?: string;
  collapsed?: boolean;
  onCollapse?: (collapsed: boolean) => void;
}

interface NavItem {
  path: string;
  label: string;
  icon?: string;
  category?: string;
  adminOnly?: boolean;
}

const Sidebar: React.FC<SidebarProps> = ({
  className = '',
  collapsed = false,
  onCollapse
}) => {
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
    { path: '/db-explorer', label: '🗄️ DB Management', category: 'System Tools' },
    { path: '/performance', label: '⚡ Performance', category: 'System Tools' },

    // Admin Features
    { path: '/admin/tuning', label: '🤖 AI Tuning', category: 'Admin', adminOnly: true },
    { path: '/admin/llm', label: '🧠 LLM Management', category: 'Admin', adminOnly: true },
    { path: '/admin/cache', label: '💾 Cache Management', category: 'Admin', adminOnly: true },
    { path: '/admin/security', label: '🔒 Security', category: 'Admin', adminOnly: true },
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
          padding: collapsed ? '8px' : '10px 20px',
          textDecoration: 'none',
          color: isActive(item.path) ? '#1890ff' : '#666',
          backgroundColor: isActive(item.path) ? '#e6f7ff' : 'transparent',
          borderRight: isActive(item.path) ? '3px solid #1890ff' : 'none',
          fontWeight: isActive(item.path) ? 600 : 400,
          fontSize: collapsed ? '16px' : '14px',
          borderRadius: '4px',
          margin: '2px 8px',
          transition: 'all 0.2s ease',
          textAlign: collapsed ? 'center' : 'left',
          overflow: 'hidden',
          whiteSpace: 'nowrap',
          height: collapsed ? '32px' : 'auto',
          lineHeight: collapsed ? '16px' : 'normal',
          display: 'flex',
          alignItems: 'center',
          justifyContent: collapsed ? 'center' : 'flex-start',
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
        title={collapsed ? item.label : undefined}
      >
        {collapsed ? item.label.split(' ')[0] : item.label}
      </Link>
    </li>
  );

  const renderCategory = (categoryName: string, items: NavItem[]) => {
    const isCategoryCollapsed = collapsedCategories.has(categoryName);

    if (categoryName === 'Main') {
      return (
        <div key={categoryName} style={{ marginBottom: '20px' }}>
          <ul style={{ listStyle: 'none', margin: 0, padding: 0 }}>
            {items.map(renderNavItem)}
          </ul>
        </div>
      );
    }

    if (collapsed) {
      // In collapsed sidebar, show only dots for categories
      return (
        <div key={categoryName} style={{ marginBottom: '8px' }}>
          {categoryName !== 'Main' && (
            <div
              style={{
                padding: '4px 8px',
                textAlign: 'center',
                marginBottom: '4px',
              }}
              title={categoryName}
            >
              <div
                style={{
                  width: '4px',
                  height: '4px',
                  borderRadius: '50%',
                  backgroundColor: '#ccc',
                  margin: '0 auto',
                }}
              />
            </div>
          )}
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
            transform: isCategoryCollapsed ? 'rotate(-90deg)' : 'rotate(0deg)',
            transition: 'transform 0.2s ease'
          }}>
            ▼
          </span>
        </div>
        {!isCategoryCollapsed && (
          <ul style={{ listStyle: 'none', margin: 0, padding: 0 }}>
            {items.map(renderNavItem)}
          </ul>
        )}
      </div>
    );
  };

  return (
    <div
      className={`sidebar ${collapsed ? 'collapsed' : ''} ${className}`}
      style={{
        width: collapsed ? '80px' : '280px',
        minHeight: '100vh',
        backgroundColor: '#fafafa',
        borderRight: '1px solid #e8e8e8',
        padding: '20px 0',
        overflowY: 'auto',
        transition: 'width 0.3s ease',
        position: 'relative',
      }}
    >
      {/* Collapse Toggle Button */}
      <div
        onClick={() => onCollapse?.(!collapsed)}
        style={{
          position: 'absolute',
          top: '20px',
          right: collapsed ? '10px' : '20px',
          width: '24px',
          height: '24px',
          borderRadius: '3px',
          background: '#f5f5f5',
          border: '1px solid #d9d9d9',
          display: 'flex',
          alignItems: 'center',
          justifyContent: 'center',
          cursor: 'pointer',
          transition: 'all 0.2s ease',
          fontSize: '12px',
          color: '#666',
          zIndex: 10,
        }}
        onMouseEnter={(e) => {
          e.currentTarget.style.background = '#e6f7ff';
          e.currentTarget.style.borderColor = '#91d5ff';
          e.currentTarget.style.color = '#1890ff';
        }}
        onMouseLeave={(e) => {
          e.currentTarget.style.background = '#f5f5f5';
          e.currentTarget.style.borderColor = '#d9d9d9';
          e.currentTarget.style.color = '#666';
        }}
      >
        {collapsed ? '→' : '←'}
      </div>

      <div style={{
        padding: collapsed ? '0 10px' : '0 20px',
        marginBottom: '30px',
        marginTop: '50px'
      }}>
        {!collapsed ? (
          <>
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
          </>
        ) : (
          <div style={{
            textAlign: 'center',
            fontSize: '20px',
            color: '#1890ff'
          }}>
            🤖
          </div>
        )}
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
          left: collapsed ? '10px' : '20px',
          right: collapsed ? '10px' : '20px',
          padding: collapsed ? '8px' : '12px',
          backgroundColor: '#e6f7ff',
          borderRadius: '6px',
          border: '1px solid #91d5ff',
          textAlign: collapsed ? 'center' : 'left'
        }}>
          {collapsed ? (
            <div style={{ fontSize: '16px' }} title="Admin Access">
              👑
            </div>
          ) : (
            <>
              <div style={{ fontSize: '12px', color: '#1890ff', fontWeight: 600 }}>
                👑 Admin Access
              </div>
              <div style={{ fontSize: '11px', color: '#666', marginTop: '2px' }}>
                Full system access enabled
              </div>
            </>
          )}
        </div>
      )}
    </div>
  );
};

export default Sidebar;

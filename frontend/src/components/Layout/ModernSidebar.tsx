/**
 * Modern Sidebar Component
 *
 * Complete sidebar navigation component with all available features.
 */

import React from 'react';
import { Link, useLocation } from 'react-router-dom';

export interface ModernSidebarProps {
  className?: string;
}

interface NavItem {
  path: string;
  label: string;
  icon?: string;
  category?: string;
  adminOnly?: boolean;
}

const ModernSidebar: React.FC<ModernSidebarProps> = ({ className = '' }) => {
  const location = useLocation();

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

  return (
    <div 
      className={`modern-sidebar ${className}`}
      style={{
        width: '240px',
        minHeight: '100vh',
        backgroundColor: '#f5f5f5',
        borderRight: '1px solid #e0e0e0',
        padding: '20px 0',
      }}
    >
      <div style={{ padding: '0 20px', marginBottom: '30px' }}>
        <h2 style={{ margin: 0, fontSize: '18px', fontWeight: 600 }}>
          BI Reporting Copilot
        </h2>
      </div>
      
      <nav>
        <ul style={{ listStyle: 'none', margin: 0, padding: 0 }}>
          {navItems.map((item) => (
            <li key={item.path}>
              <Link
                to={item.path}
                style={{
                  display: 'block',
                  padding: '12px 20px',
                  textDecoration: 'none',
                  color: isActive(item.path) ? '#1890ff' : '#666',
                  backgroundColor: isActive(item.path) ? '#e6f7ff' : 'transparent',
                  borderRight: isActive(item.path) ? '3px solid #1890ff' : 'none',
                  fontWeight: isActive(item.path) ? 600 : 400,
                }}
              >
                {item.label}
              </Link>
            </li>
          ))}
        </ul>
      </nav>
    </div>
  );
};

export default ModernSidebar;

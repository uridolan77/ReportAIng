import React, { useState, useEffect, useRef } from 'react';
import { Modal, Input, List, Typography, Space, Tag, Divider } from 'antd';
import {
  SearchOutlined,
  PlayCircleOutlined,
  HistoryOutlined,
  BookOutlined,
  SettingOutlined,
  ExportOutlined,
  BulbOutlined,
  ThunderboltOutlined,
  DatabaseOutlined,
  BarChartOutlined
} from '@ant-design/icons';

const { Text } = Typography;
const { Search } = Input;

interface Command {
  id: string;
  title: string;
  description: string;
  category: string;
  icon: React.ReactNode;
  shortcut?: string;
  action: () => void;
  keywords: string[];
}

interface CommandPaletteProps {
  visible: boolean;
  onClose: () => void;
  commands?: Command[];
}

export const CommandPalette: React.FC<CommandPaletteProps> = ({
  visible,
  onClose,
  commands: customCommands = []
}) => {
  const [searchTerm, setSearchTerm] = useState('');
  const [selectedIndex, setSelectedIndex] = useState(0);
  const [filteredCommands, setFilteredCommands] = useState<Command[]>([]);
  const searchInputRef = useRef<any>(null);

  // Default commands
  const defaultCommands: Command[] = [
    {
      id: 'execute-query',
      title: 'Execute Query',
      description: 'Run the current query',
      category: 'Query',
      icon: <PlayCircleOutlined />,
      shortcut: 'Ctrl+Enter',
      action: () => {
        // Trigger query execution
        const event = new CustomEvent('command-palette-action', { detail: { action: 'execute-query' } });
        window.dispatchEvent(event);
        onClose();
      },
      keywords: ['execute', 'run', 'query', 'sql']
    },
    {
      id: 'new-query',
      title: 'New Query',
      description: 'Start a new query',
      category: 'Query',
      icon: <PlayCircleOutlined />,
      shortcut: 'Ctrl+N',
      action: () => {
        const event = new CustomEvent('command-palette-action', { detail: { action: 'new-query' } });
        window.dispatchEvent(event);
        onClose();
      },
      keywords: ['new', 'create', 'query', 'fresh']
    },
    {
      id: 'save-query',
      title: 'Save Query',
      description: 'Save the current query',
      category: 'Query',
      icon: <DatabaseOutlined />,
      shortcut: 'Ctrl+S',
      action: () => {
        const event = new CustomEvent('command-palette-action', { detail: { action: 'save-query' } });
        window.dispatchEvent(event);
        onClose();
      },
      keywords: ['save', 'store', 'query']
    },
    {
      id: 'query-history',
      title: 'Query History',
      description: 'View previous queries',
      category: 'Navigation',
      icon: <HistoryOutlined />,
      shortcut: 'Ctrl+H',
      action: () => {
        const event = new CustomEvent('command-palette-action', { detail: { action: 'toggle-history' } });
        window.dispatchEvent(event);
        onClose();
      },
      keywords: ['history', 'previous', 'past', 'queries']
    },
    {
      id: 'query-templates',
      title: 'Query Templates',
      description: 'Browse query templates',
      category: 'Navigation',
      icon: <BookOutlined />,
      shortcut: 'Ctrl+T',
      action: () => {
        const event = new CustomEvent('command-palette-action', { detail: { action: 'open-templates' } });
        window.dispatchEvent(event);
        onClose();
      },
      keywords: ['templates', 'examples', 'library', 'browse']
    },
    {
      id: 'export-results',
      title: 'Export Results',
      description: 'Export query results',
      category: 'Export',
      icon: <ExportOutlined />,
      shortcut: 'Ctrl+E',
      action: () => {
        const event = new CustomEvent('command-palette-action', { detail: { action: 'export-results' } });
        window.dispatchEvent(event);
        onClose();
      },
      keywords: ['export', 'download', 'save', 'results', 'csv', 'excel']
    },
    {
      id: 'toggle-insights',
      title: 'Toggle Insights Panel',
      description: 'Show/hide AI insights',
      category: 'View',
      icon: <BulbOutlined />,
      shortcut: 'Ctrl+J',
      action: () => {
        const event = new CustomEvent('command-palette-action', { detail: { action: 'toggle-insights' } });
        window.dispatchEvent(event);
        onClose();
      },
      keywords: ['insights', 'ai', 'analysis', 'panel', 'toggle']
    },
    {
      id: 'cache-manager',
      title: 'Cache Manager',
      description: 'Manage query cache',
      category: 'Admin',
      icon: <ThunderboltOutlined />,
      action: () => {
        const event = new CustomEvent('command-palette-action', { detail: { action: 'open-cache-manager' } });
        window.dispatchEvent(event);
        onClose();
      },
      keywords: ['cache', 'performance', 'manager', 'admin']
    },
    {
      id: 'security-dashboard',
      title: 'Security Dashboard',
      description: 'View security settings',
      category: 'Admin',
      icon: <SettingOutlined />,
      action: () => {
        const event = new CustomEvent('command-palette-action', { detail: { action: 'open-security' } });
        window.dispatchEvent(event);
        onClose();
      },
      keywords: ['security', 'admin', 'settings', 'dashboard']
    },
    {
      id: 'visualization-panel',
      title: 'Advanced Visualizations',
      description: 'Open visualization panel',
      category: 'View',
      icon: <BarChartOutlined />,
      action: () => {
        const event = new CustomEvent('command-palette-action', { detail: { action: 'open-visualizations' } });
        window.dispatchEvent(event);
        onClose();
      },
      keywords: ['visualization', 'charts', 'graphs', 'visual', 'panel']
    }
  ];

  const allCommands = [...defaultCommands, ...customCommands];

  useEffect(() => {
    if (visible && searchInputRef.current) {
      setTimeout(() => {
        searchInputRef.current?.focus();
      }, 100);
    }
  }, [visible]);

  useEffect(() => {
    if (!searchTerm) {
      setFilteredCommands(allCommands);
    } else {
      const filtered = allCommands.filter(command => {
        const searchLower = searchTerm.toLowerCase();
        return (
          command.title.toLowerCase().includes(searchLower) ||
          command.description.toLowerCase().includes(searchLower) ||
          command.category.toLowerCase().includes(searchLower) ||
          command.keywords.some(keyword => keyword.toLowerCase().includes(searchLower))
        );
      });
      setFilteredCommands(filtered);
    }
    setSelectedIndex(0);
  }, [searchTerm, allCommands]);

  const handleKeyDown = (e: React.KeyboardEvent) => {
    switch (e.key) {
      case 'ArrowDown':
        e.preventDefault();
        setSelectedIndex(prev => (prev + 1) % filteredCommands.length);
        break;
      case 'ArrowUp':
        e.preventDefault();
        setSelectedIndex(prev => prev === 0 ? filteredCommands.length - 1 : prev - 1);
        break;
      case 'Enter':
        e.preventDefault();
        if (filteredCommands[selectedIndex]) {
          filteredCommands[selectedIndex].action();
        }
        break;
      case 'Escape':
        e.preventDefault();
        onClose();
        break;
    }
  };

  const handleCommandClick = (command: Command) => {
    command.action();
  };

  const getCategoryColor = (category: string) => {
    const colors: Record<string, string> = {
      'Query': 'blue',
      'Navigation': 'green',
      'Export': 'orange',
      'View': 'purple',
      'Admin': 'red',
      'Edit': 'cyan'
    };
    return colors[category] || 'default';
  };

  const groupedCommands = filteredCommands.reduce((groups, command) => {
    const category = command.category;
    if (!groups[category]) {
      groups[category] = [];
    }
    groups[category].push(command);
    return groups;
  }, {} as Record<string, Command[]>);

  return (
    <Modal
      title={null}
      open={visible}
      onCancel={onClose}
      footer={null}
      width={600}
      centered
      bodyStyle={{ padding: 0 }}
      destroyOnClose
    >
      <div style={{ padding: '16px 16px 0 16px' }}>
        <Search
          ref={searchInputRef}
          placeholder="Type a command or search..."
          value={searchTerm}
          onChange={(e) => setSearchTerm(e.target.value)}
          onKeyDown={handleKeyDown}
          size="large"
          prefix={<SearchOutlined />}
          style={{ marginBottom: '16px' }}
        />
      </div>

      <div style={{ maxHeight: '400px', overflowY: 'auto' }}>
        {Object.keys(groupedCommands).length === 0 ? (
          <div style={{ padding: '40px', textAlign: 'center' }}>
            <Text type="secondary">No commands found</Text>
          </div>
        ) : (
          Object.entries(groupedCommands).map(([category, commands], categoryIndex) => (
            <div key={category}>
              {categoryIndex > 0 && <Divider style={{ margin: '8px 0' }} />}

              <div style={{ padding: '8px 16px 4px 16px' }}>
                <Text strong style={{ fontSize: '12px', color: '#8c8c8c' }}>
                  {category.toUpperCase()}
                </Text>
              </div>

              <List
                dataSource={commands}
                renderItem={(command, index) => {
                  const globalIndex = filteredCommands.findIndex(c => c.id === command.id);
                  const isSelected = globalIndex === selectedIndex;

                  return (
                    <List.Item
                      style={{
                        padding: '8px 16px',
                        cursor: 'pointer',
                        backgroundColor: isSelected ? '#f0f0f0' : 'transparent',
                        borderLeft: isSelected ? '3px solid #1890ff' : '3px solid transparent'
                      }}
                      onClick={() => handleCommandClick(command)}
                      onMouseEnter={() => setSelectedIndex(globalIndex)}
                    >
                      <List.Item.Meta
                        avatar={
                          <div style={{
                            fontSize: '16px',
                            color: isSelected ? '#1890ff' : '#8c8c8c',
                            marginTop: '2px'
                          }}>
                            {command.icon}
                          </div>
                        }
                        title={
                          <Space>
                            <Text strong style={{ fontSize: '14px' }}>
                              {command.title}
                            </Text>
                            <Tag color={getCategoryColor(command.category)} style={{ fontSize: '11px' }}>
                              {command.category}
                            </Tag>
                          </Space>
                        }
                        description={
                          <Text type="secondary" style={{ fontSize: '12px' }}>
                            {command.description}
                          </Text>
                        }
                      />
                      {command.shortcut && (
                        <div style={{
                          fontSize: '11px',
                          color: '#8c8c8c',
                          fontFamily: 'monospace',
                          backgroundColor: '#f5f5f5',
                          padding: '2px 6px',
                          borderRadius: '3px'
                        }}>
                          {command.shortcut}
                        </div>
                      )}
                    </List.Item>
                  );
                }}
              />
            </div>
          ))
        )}
      </div>

      <div style={{
        padding: '12px 16px',
        borderTop: '1px solid #f0f0f0',
        backgroundColor: '#fafafa'
      }}>
        <Space size="large">
          <Text type="secondary" style={{ fontSize: '11px' }}>
            <kbd>↑↓</kbd> Navigate
          </Text>
          <Text type="secondary" style={{ fontSize: '11px' }}>
            <kbd>Enter</kbd> Execute
          </Text>
          <Text type="secondary" style={{ fontSize: '11px' }}>
            <kbd>Esc</kbd> Close
          </Text>
        </Space>
      </div>
    </Modal>
  );
};

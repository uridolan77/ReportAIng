/**
 * LLM Provider and Model Selector Component
 * 
 * Allows users to select which LLM provider and model to use for their queries.
 * Integrates with the LLM Management system to show available options.
 */

import React, { useState, useEffect, useCallback } from 'react';
import { Select, Space, Tooltip, Badge, Button, Card, Flex } from 'antd';
import { 
  ApiOutlined, 
  RobotOutlined, 
  SettingOutlined,
  InfoCircleOutlined,
  ThunderboltOutlined
} from '@ant-design/icons';
import { 
  llmManagementService, 
  LLMProviderConfig, 
  LLMModelConfig, 
  LLMProviderStatus 
} from '../../services/llmManagementService';

const { Option } = Select;

interface LLMSelectorProps {
  selectedProviderId?: string;
  selectedModelId?: string;
  useCase?: string;
  onProviderChange?: (providerId: string) => void;
  onModelChange?: (modelId: string) => void;
  compact?: boolean;
  showStatus?: boolean;
}

export const LLMSelector: React.FC<LLMSelectorProps> = ({
  selectedProviderId,
  selectedModelId,
  useCase = 'SQL',
  onProviderChange,
  onModelChange,
  compact = false,
  showStatus = true
}) => {
  const [providers, setProviders] = useState<LLMProviderConfig[]>([]);
  const [models, setModels] = useState<LLMModelConfig[]>([]);
  const [providerHealth, setProviderHealth] = useState<LLMProviderStatus[]>([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    loadData();
  }, [loadData]);

  useEffect(() => {
    if (selectedProviderId) {
      loadModelsForProvider(selectedProviderId);
    }
  }, [selectedProviderId, useCase, loadModelsForProvider]);

  const loadData = useCallback(async () => {
    try {
      setLoading(true);
      const [providersData, healthData] = await Promise.all([
        llmManagementService.getProviders(),
        llmManagementService.getProviderHealth()
      ]);

      // Only show enabled providers
      const enabledProviders = providersData.filter(p => p.isEnabled);
      setProviders(enabledProviders);
      setProviderHealth(healthData);

      // Auto-select default provider if none selected
      if (!selectedProviderId && enabledProviders.length > 0) {
        const defaultProvider = enabledProviders.find(p => p.isDefault) || enabledProviders[0];
        onProviderChange?.(defaultProvider.providerId);
      }
    } catch (error) {
      console.error('Failed to load LLM data:', error);
    } finally {
      setLoading(false);
    }
  }, [selectedProviderId, onProviderChange]);

  const loadModelsForProvider = useCallback(async (providerId: string) => {
    try {
      const modelsData = await llmManagementService.getModels(providerId);

      // Filter by use case and enabled status
      const filteredModels = modelsData.filter(m =>
        m.isEnabled && (m.useCase === useCase || m.useCase === '')
      );

      setModels(filteredModels);

      // Auto-select first model if none selected
      if (!selectedModelId && filteredModels.length > 0) {
        onModelChange?.(filteredModels[0].modelId);
      }
    } catch (error) {
      console.error('Failed to load models:', error);
    }
  }, [useCase, selectedModelId, onModelChange]);

  const getProviderStatus = (providerId: string) => {
    const health = providerHealth.find(h => h.providerId === providerId);
    if (!health) return { status: 'default', text: 'Unknown' };
    
    if (health.isHealthy) {
      return { status: 'success', text: 'Healthy' };
    } else {
      return { status: 'error', text: 'Unhealthy' };
    }
  };

  const formatCost = (costPerToken: number) => {
    if (costPerToken === 0) return 'Free';
    if (costPerToken < 0.000001) return '< $0.000001/token';
    return `$${costPerToken.toFixed(6)}/token`;
  };

  const formatResponseTime = (providerId: string) => {
    const health = providerHealth.find(h => h.providerId === providerId);
    if (!health || health.lastResponseTime === 0) return '';
    
    if (health.lastResponseTime < 1000) {
      return `${health.lastResponseTime}ms`;
    }
    return `${(health.lastResponseTime / 1000).toFixed(1)}s`;
  };

  if (compact) {
    return (
      <Space size="small">
        <Select
          value={selectedProviderId}
          onChange={onProviderChange}
          loading={loading}
          style={{ minWidth: 120 }}
          size="small"
          placeholder="Provider"
        >
          {providers.map(provider => (
            <Option key={provider.providerId} value={provider.providerId}>
              <Space>
                <ApiOutlined />
                {provider.name}
                {showStatus && (
                  <Badge 
                    status={getProviderStatus(provider.providerId).status as any} 
                    size="small"
                  />
                )}
              </Space>
            </Option>
          ))}
        </Select>

        <Select
          value={selectedModelId}
          onChange={onModelChange}
          loading={loading}
          style={{ minWidth: 150 }}
          size="small"
          placeholder="Model"
          disabled={!selectedProviderId}
        >
          {models.map(model => (
            <Option key={model.modelId} value={model.modelId}>
              <Space>
                <RobotOutlined />
                {model.displayName}
              </Space>
            </Option>
          ))}
        </Select>
      </Space>
    );
  }

  return (
    <Card size="small" style={{ marginBottom: '16px' }}>
      <Flex justify="between" align="center" style={{ marginBottom: '12px' }}>
        <div>
          <h4 style={{ margin: 0, display: 'flex', alignItems: 'center', gap: '8px' }}>
            <SettingOutlined />
            AI Configuration
          </h4>
          <p style={{ margin: 0, color: '#666', fontSize: '12px' }}>
            Select provider and model for {useCase.toLowerCase()} generation
          </p>
        </div>
        <Button 
          type="link" 
          size="small" 
          icon={<InfoCircleOutlined />}
          onClick={() => window.open('/admin/llm', '_blank')}
        >
          Manage
        </Button>
      </Flex>

      <Space direction="vertical" style={{ width: '100%' }}>
        {/* Provider Selection */}
        <div>
          <label style={{ fontSize: '12px', fontWeight: 'bold', marginBottom: '4px', display: 'block' }}>
            Provider
          </label>
          <Select
            value={selectedProviderId}
            onChange={onProviderChange}
            loading={loading}
            style={{ width: '100%' }}
            placeholder="Select AI Provider"
          >
            {providers.map(provider => {
              const status = getProviderStatus(provider.providerId);
              const responseTime = formatResponseTime(provider.providerId);
              
              return (
                <Option key={provider.providerId} value={provider.providerId}>
                  <Flex justify="between" align="center">
                    <Space>
                      <ApiOutlined />
                      <div>
                        <div style={{ fontWeight: 'bold' }}>{provider.name}</div>
                        <div style={{ fontSize: '11px', color: '#666' }}>{provider.type}</div>
                      </div>
                    </Space>
                    <Space>
                      {responseTime && (
                        <Tooltip title="Average response time">
                          <Badge 
                            count={responseTime} 
                            style={{ backgroundColor: '#52c41a' }}
                            size="small"
                          />
                        </Tooltip>
                      )}
                      <Badge status={status.status as any} text={status.text} />
                    </Space>
                  </Flex>
                </Option>
              );
            })}
          </Select>
        </div>

        {/* Model Selection */}
        <div>
          <label style={{ fontSize: '12px', fontWeight: 'bold', marginBottom: '4px', display: 'block' }}>
            Model
          </label>
          <Select
            value={selectedModelId}
            onChange={onModelChange}
            loading={loading}
            style={{ width: '100%' }}
            placeholder="Select Model"
            disabled={!selectedProviderId}
          >
            {models.map(model => (
              <Option key={model.modelId} value={model.modelId}>
                <Flex justify="between" align="center">
                  <Space>
                    <RobotOutlined />
                    <div>
                      <div style={{ fontWeight: 'bold' }}>{model.displayName}</div>
                      <div style={{ fontSize: '11px', color: '#666' }}>
                        Temp: {model.temperature} | Max: {model.maxTokens} tokens
                      </div>
                    </div>
                  </Space>
                  <Space>
                    <Tooltip title="Cost per token">
                      <Badge 
                        count={formatCost(model.costPerToken)} 
                        style={{ backgroundColor: '#fa8c16' }}
                        size="small"
                      />
                    </Tooltip>
                    <Tooltip title="Performance optimized">
                      <ThunderboltOutlined style={{ color: '#52c41a' }} />
                    </Tooltip>
                  </Space>
                </Flex>
              </Option>
            ))}
          </Select>
        </div>
      </Space>
    </Card>
  );
};

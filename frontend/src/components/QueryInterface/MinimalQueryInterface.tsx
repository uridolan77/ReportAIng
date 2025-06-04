/**
 * Minimal Query Interface - Clean, focused main page
 * Features only the essential query input and immediate results
 */

import React, { useState, useEffect } from 'react';
import {
  Typography,
  Space,
  Button,
  Row,
  Col,
  Empty,
  Tag
} from 'antd';
import {
  HistoryOutlined,
  BookOutlined,
  ThunderboltOutlined,
  RocketOutlined
} from '@ant-design/icons';
import { useLocation } from 'react-router-dom';
import { useQueryContext } from './QueryProvider';
import { EnhancedQueryInput } from './EnhancedQueryInput';
import { QueryTabs } from './QueryTabs';
import { OnboardingTour } from '../Onboarding/OnboardingTour';
import { ProactiveSuggestions } from './ProactiveSuggestions';
import { GuidedQueryWizard } from './GuidedQueryWizard';
import { ErrorRecoveryPanel, QueryValidator } from './EnhancedErrorHandling';
import { AIProcessingFeedback, useAIProcessingFeedback, createProcessingStepFromError } from './AIProcessingFeedback';
import { AccessibilityFeatures } from './AccessibilityFeatures';
import './animations.css';
import './professional-polish.css';

const { Title, Text, Paragraph } = Typography;

export const MinimalQueryInterface: React.FC = () => {
  const location = useLocation();
  const {
    query,
    setQuery,
    currentResult,
    isLoading,
    isConnected,
    handleSubmitQuery,
    setShowTemplateLibrary,
    setActiveTab,
    queryHistory
  } = useQueryContext();

  const [showQuickActions] = useState(true);
  const [isFirstVisit, setIsFirstVisit] = useState(false);
  const [showWizard, setShowWizard] = useState(false);
  const [showProactiveSuggestions, setShowProactiveSuggestions] = useState(true);
  const [queryError, setQueryError] = useState<any>(null);
  const [validationResult, setValidationResult] = useState<any>(null);

  // AI Processing feedback
  const aiProcessing = useAIProcessingFeedback();

  // Accessibility action handler
  const handleAccessibilityAction = (action: string) => {
    switch (action) {
      case 'execute-query':
        handleSubmitQuery();
        break;
      case 'open-wizard':
        setShowWizard(true);
        break;
      case 'clear-query':
        setQuery('');
        break;
      case 'export-results':
        // Handle export
        break;
      case 'save-query':
        // Handle save
        break;
      case 'toggle-fullscreen':
        // Handle fullscreen
        break;
    }
  };

  // Handle navigation state (suggested queries from other pages)
  useEffect(() => {
    const state = location.state as { suggestedQuery?: string } | null;
    if (state?.suggestedQuery) {
      setQuery(state.suggestedQuery);
      // Clear the navigation state to prevent re-setting on subsequent renders
      window.history.replaceState({}, document.title);
    }
  }, [location.state, setQuery]);

  // Check if this is user's first visit
  useEffect(() => {
    const hasVisited = localStorage.getItem('has-visited-app');
    if (!hasVisited) {
      setIsFirstVisit(true);
      localStorage.setItem('has-visited-app', 'true');
    }
  }, []);

  const quickActions = [
    {
      key: 'templates',
      icon: <BookOutlined />,
      title: 'Query Templates',
      description: 'Browse pre-built queries',
      action: () => setShowTemplateLibrary(true),
      color: '#3b82f6',
      bgGradient: 'linear-gradient(135deg, #eff6ff 0%, #dbeafe 100%)',
      category: 'Templates'
    },
    {
      key: 'history',
      icon: <HistoryOutlined />,
      title: 'Recent Queries',
      description: `${Array.isArray(queryHistory) ? queryHistory.length : 0} saved queries`,
      action: () => setActiveTab('history'),
      color: '#10b981',
      bgGradient: 'linear-gradient(135deg, #ecfdf5 0%, #d1fae5 100%)',
      category: 'History'
    },
    {
      key: 'examples',
      icon: <RocketOutlined />,
      title: 'Quick Examples',
      description: 'Try sample queries',
      action: () => setQuery('Show me total deposits for yesterday'),
      color: '#8b5cf6',
      bgGradient: 'linear-gradient(135deg, #f3e8ff 0%, #e9d5ff 100%)',
      category: 'Examples'
    }
  ];

  const exampleQueries = [
    {
      category: "💰 Sales & Revenue",
      queries: [
        "Show me total deposits for yesterday",
        "Show me daily revenue for the last week",
        "Revenue breakdown by country for last week"
      ],
      color: "#10b981",
      bgGradient: "linear-gradient(135deg, #ecfdf5 0%, #d1fae5 100%)"
    },
    {
      category: "👥 Player Analytics",
      queries: [
        "Top 10 players by deposits in the last 7 days",
        "Count of active players yesterday",
        "Show me casino vs sports betting revenue for last week"
      ],
      color: "#3b82f6",
      bgGradient: "linear-gradient(135deg, #eff6ff 0%, #dbeafe 100%)"
    }
  ];

  return (
    <div
      className="query-interface-container"
      style={{ width: '100%', margin: '0', padding: '40px 24px' }}
    >
      {/* Enhanced Hero Section */}
      <div style={{ textAlign: 'center', marginBottom: '72px', padding: '0 16px' }}>
        <Title
          level={1}
          style={{
            fontSize: '4.5rem',
            fontWeight: 900,
            margin: '0 0 24px 0',
            background: 'linear-gradient(135deg, #3b82f6 0%, #1d4ed8 50%, #7c3aed 100%)',
            WebkitBackgroundClip: 'text',
            WebkitTextFillColor: 'transparent',
            backgroundClip: 'text',
            lineHeight: '1.1',
            fontFamily: "'Poppins', sans-serif",
            letterSpacing: '-0.02em',
            textShadow: '0 4px 8px rgba(59, 130, 246, 0.1)'
          }}
        >
          ✨ Ask Your Data Anything
        </Title>
        <Paragraph
          style={{
            fontSize: '20px',
            color: '#4b5563',
            maxWidth: '700px',
            margin: '0 auto 32px',
            lineHeight: '1.7',
            fontWeight: 500,
            fontFamily: "'Inter', sans-serif"
          }}
        >
          Transform your business questions into powerful insights using natural language.
          <br />
          <span style={{ color: '#6b7280', fontSize: '18px', fontWeight: 400 }}>
            No SQL knowledge required • Powered by AI
          </span>
        </Paragraph>

        {!isConnected && (
          <Tag
            color="orange"
            style={{
              fontSize: '14px',
              padding: '6px 16px',
              borderRadius: '20px',
              fontWeight: 500
            }}
          >
            Working in offline mode
          </Tag>
        )}
      </div>

      {/* Proactive Suggestions - Show when no query and no results */}
      {!currentResult && !query && showProactiveSuggestions && (
        <ProactiveSuggestions
          onQuerySelect={(selectedQuery) => {
            setQuery(selectedQuery);
            setShowProactiveSuggestions(false);
          }}
          onStartWizard={() => setShowWizard(true)}
          recentQueries={Array.isArray(queryHistory) ? queryHistory.map(h => h.query || '').slice(0, 5) : []}
        />
      )}

      {/* Enhanced Query Input Area */}
      <div
        style={{
          marginBottom: '64px',
          background: 'linear-gradient(135deg, #ffffff 0%, #f8fafc 100%)',
          border: '2px solid #e2e8f0',
          borderRadius: '24px',
          padding: '40px',
          boxShadow: '0 8px 32px rgba(59, 130, 246, 0.08), 0 1px 2px rgba(0, 0, 0, 0.05)',
          transition: 'all 0.4s cubic-bezier(0.4, 0, 0.2, 1)',
          position: 'relative',
          overflow: 'hidden'
        }}
        onMouseEnter={(e) => {
          e.currentTarget.style.borderColor = '#3b82f6';
          e.currentTarget.style.boxShadow = '0 12px 48px rgba(59, 130, 246, 0.15), 0 4px 8px rgba(0, 0, 0, 0.1)';
          e.currentTarget.style.transform = 'translateY(-2px)';
        }}
        onMouseLeave={(e) => {
          e.currentTarget.style.borderColor = '#e2e8f0';
          e.currentTarget.style.boxShadow = '0 8px 32px rgba(59, 130, 246, 0.08), 0 1px 2px rgba(0, 0, 0, 0.05)';
          e.currentTarget.style.transform = 'translateY(0)';
        }}
      >
        {/* AI Indicator */}
        <div style={{
          display: 'flex',
          alignItems: 'center',
          justifyContent: 'center',
          marginBottom: '20px',
          gap: '8px'
        }}>
          <div style={{
            width: '8px',
            height: '8px',
            borderRadius: '50%',
            background: 'linear-gradient(135deg, #10b981 0%, #059669 100%)',
            animation: 'pulse 2s infinite'
          }} />
          <Text style={{
            fontSize: '14px',
            color: '#059669',
            fontWeight: 600,
            textTransform: 'uppercase',
            letterSpacing: '0.05em',
            fontFamily: "'Inter', sans-serif"
          }}>
            AI-Powered Natural Language Query
          </Text>
          <div style={{
            width: '8px',
            height: '8px',
            borderRadius: '50%',
            background: 'linear-gradient(135deg, #10b981 0%, #059669 100%)',
            animation: 'pulse 2s infinite 1s'
          }} />
        </div>

        <div data-testid="query-input">
          <EnhancedQueryInput
            value={query}
            onChange={setQuery}
            onSubmit={handleSubmitQuery}
            loading={isLoading}
            placeholder="Ask a question about your data... (e.g., 'Show me revenue by country last month')"
            showShortcuts={false}
            autoHeight={true}
            maxRows={4}
          />
        </div>

        {/* Query Validator */}
        <QueryValidator
          query={query}
          onValidationResult={setValidationResult}
        />

        {/* AI Processing Feedback */}
        {(isLoading || aiProcessing.isProcessing) && (
          <div style={{ marginTop: '16px' }}>
            <AIProcessingFeedback
              isProcessing={isLoading || aiProcessing.isProcessing}
              currentStep={aiProcessing.currentStep}
              steps={aiProcessing.steps}
              showDetails={false}
            />
          </div>
        )}

        {/* Validation Feedback - Only show actual errors, not AI processing steps */}
        {validationResult && !validationResult.isValid && !isLoading && (
          <div style={{ marginTop: '16px' }}>
            {validationResult.errors
              .filter((error: any) => !error.isProcessingStep && error.type !== 'ai_processing')
              .map((error: any, index: number) => (
                <ErrorRecoveryPanel
                  key={index}
                  error={error}
                  originalQuery={query}
                  onRetry={() => handleSubmitQuery()}
                  onQueryFix={(fixedQuery) => setQuery(fixedQuery)}
                  onGetHelp={() => setShowWizard(true)}
                />
              ))}
          </div>
        )}
      </div>

      {/* Enhanced Quick Actions */}
      {showQuickActions && !currentResult && (
        <div style={{ marginBottom: '72px' }}>
          <div style={{
            textAlign: 'center',
            marginBottom: '48px'
          }}>
            <Text style={{
              fontSize: '28px',
              fontWeight: 700,
              color: '#1f2937',
              display: 'block',
              marginBottom: '12px',
              fontFamily: "'Poppins', sans-serif"
            }}>
              🚀 Quick Actions
            </Text>
            <Text style={{
              fontSize: '18px',
              color: '#6b7280',
              fontWeight: 500
            }}>
              Get started with these powerful features
            </Text>
          </div>

          <Row gutter={[32, 32]} justify="center">
            {quickActions.map((action, index) => (
              <Col xs={24} sm={8} key={action.key}>
                <div
                  style={{
                    padding: '32px 28px',
                    background: action.bgGradient,
                    borderRadius: '20px',
                    cursor: 'pointer',
                    transition: 'all 0.4s cubic-bezier(0.4, 0, 0.2, 1)',
                    textAlign: 'center',
                    border: `2px solid ${action.color}20`,
                    position: 'relative',
                    overflow: 'hidden',
                    height: '200px',
                    display: 'flex',
                    flexDirection: 'column',
                    justifyContent: 'center',
                    alignItems: 'center'
                  }}
                  onClick={action.action}
                  onMouseEnter={(e) => {
                    e.currentTarget.style.transform = 'translateY(-8px) scale(1.02)';
                    e.currentTarget.style.boxShadow = `0 20px 60px ${action.color}30`;
                    e.currentTarget.style.borderColor = action.color;
                  }}
                  onMouseLeave={(e) => {
                    e.currentTarget.style.transform = 'translateY(0) scale(1)';
                    e.currentTarget.style.boxShadow = 'none';
                    e.currentTarget.style.borderColor = `${action.color}20`;
                  }}
                >
                  {/* Category Badge */}
                  <div style={{
                    position: 'absolute',
                    top: '16px',
                    right: '16px',
                    background: action.color,
                    color: 'white',
                    padding: '4px 8px',
                    borderRadius: '12px',
                    fontSize: '10px',
                    fontWeight: 600,
                    textTransform: 'uppercase',
                    letterSpacing: '0.05em'
                  }}>
                    {action.category}
                  </div>

                  <div style={{
                    fontSize: '56px',
                    color: action.color,
                    marginBottom: '20px',
                    transition: 'all 0.3s ease',
                    filter: 'drop-shadow(0 4px 8px rgba(0, 0, 0, 0.1))'
                  }}>
                    {action.icon}
                  </div>
                  <Text style={{
                    fontSize: '20px',
                    fontWeight: 700,
                    color: '#1f2937',
                    display: 'block',
                    marginBottom: '8px',
                    fontFamily: "'Poppins', sans-serif"
                  }}>
                    {action.title}
                  </Text>
                  <Text style={{
                    fontSize: '15px',
                    color: '#4b5563',
                    lineHeight: '1.5',
                    fontWeight: 500
                  }}>
                    {action.description}
                  </Text>
                </div>
              </Col>
            ))}
          </Row>
        </div>
      )}

      {/* Enhanced Example Queries by Category */}
      {!currentResult && (
        <div style={{ marginBottom: '72px' }}>
          <div style={{
            textAlign: 'center',
            marginBottom: '48px'
          }}>
            <Text style={{
              fontSize: '28px',
              fontWeight: 700,
              color: '#1f2937',
              display: 'block',
              marginBottom: '12px',
              fontFamily: "'Poppins', sans-serif"
            }}>
              ⚡ Try These Examples
            </Text>
            <Text style={{
              fontSize: '18px',
              color: '#6b7280',
              fontWeight: 500
            }}>
              Organized by category for easy discovery
            </Text>
          </div>

          {exampleQueries.map((category, categoryIndex) => (
            <div key={categoryIndex} style={{ marginBottom: '40px' }}>
              {/* Category Header */}
              <div style={{
                textAlign: 'center',
                marginBottom: '24px'
              }}>
                <Text style={{
                  fontSize: '20px',
                  fontWeight: 600,
                  color: category.color,
                  display: 'block',
                  fontFamily: "'Poppins', sans-serif"
                }}>
                  {category.category}
                </Text>
              </div>

              {/* Category Queries */}
              <Row gutter={[20, 20]} justify="center">
                {category.queries.map((query, queryIndex) => (
                  <Col xs={24} sm={12} lg={8} key={queryIndex}>
                    <Button
                      type="text"
                      onClick={() => setQuery(query)}
                      style={{
                        width: '100%',
                        textAlign: 'left',
                        height: 'auto',
                        padding: '24px',
                        background: category.bgGradient,
                        border: `2px solid ${category.color}20`,
                        borderRadius: '16px',
                        color: '#1f2937',
                        whiteSpace: 'normal',
                        lineHeight: '1.6',
                        fontSize: '15px',
                        fontWeight: 500,
                        transition: 'all 0.4s cubic-bezier(0.4, 0, 0.2, 1)',
                        minHeight: '100px',
                        display: 'flex',
                        alignItems: 'center',
                        position: 'relative',
                        overflow: 'hidden'
                      }}
                      onMouseEnter={(e) => {
                        e.currentTarget.style.background = `linear-gradient(135deg, ${category.color} 0%, ${category.color}dd 100%)`;
                        e.currentTarget.style.color = 'white';
                        e.currentTarget.style.borderColor = category.color;
                        e.currentTarget.style.transform = 'translateY(-4px) scale(1.02)';
                        e.currentTarget.style.boxShadow = `0 12px 32px ${category.color}40`;
                      }}
                      onMouseLeave={(e) => {
                        e.currentTarget.style.background = category.bgGradient;
                        e.currentTarget.style.color = '#1f2937';
                        e.currentTarget.style.borderColor = `${category.color}20`;
                        e.currentTarget.style.transform = 'translateY(0) scale(1)';
                        e.currentTarget.style.boxShadow = 'none';
                      }}
                    >
                      <div style={{
                        position: 'absolute',
                        top: '8px',
                        right: '8px',
                        width: '6px',
                        height: '6px',
                        borderRadius: '50%',
                        background: category.color,
                        opacity: 0.6
                      }} />
                      {query}
                    </Button>
                  </Col>
                ))}
              </Row>
            </div>
          ))}
        </div>
      )}

      {/* Results Section */}
      {currentResult && (
        <div
          data-testid="results-area"
          tabIndex={0}
          style={{
            width: '100%',
            background: '#ffffff',
            border: '2px solid #f1f5f9',
            borderRadius: '16px',
            padding: '32px',
            boxShadow: '0 4px 20px rgba(0, 0, 0, 0.08)'
          }}
        >
          <QueryTabs />
        </div>
      )}

      {/* Enhanced Empty State */}
      {!currentResult && !isLoading && (
        <div style={{
          textAlign: 'center',
          padding: '80px 24px',
          background: 'linear-gradient(135deg, #f8fafc 0%, #f1f5f9 100%)',
          borderRadius: '24px',
          margin: '40px 0',
          border: '2px dashed #e2e8f0',
          position: 'relative',
          overflow: 'hidden'
        }}>
          {/* Background decoration */}
          <div style={{
            position: 'absolute',
            top: '-50%',
            left: '-50%',
            width: '200%',
            height: '200%',
            background: 'radial-gradient(circle, rgba(59, 130, 246, 0.03) 0%, transparent 70%)',
            animation: 'rotate 20s linear infinite'
          }} />

          <div style={{
            fontSize: '72px',
            marginBottom: '24px',
            animation: 'float 3s ease-in-out infinite',
            position: 'relative',
            zIndex: 1
          }}>
            🚀
          </div>
          <Text style={{
            fontSize: '24px',
            color: '#1f2937',
            fontWeight: 700,
            display: 'block',
            marginBottom: '12px',
            position: 'relative',
            zIndex: 1,
            fontFamily: "'Poppins', sans-serif"
          }}>
            Ready to Explore Your Data
          </Text>
          <Text style={{
            fontSize: '18px',
            color: '#6b7280',
            fontWeight: 500,
            display: 'block',
            marginBottom: '32px',
            lineHeight: '1.6'
          }}>
            Ask questions in natural language and get instant insights
            <br />
            <span style={{ fontSize: '16px', color: '#9ca3af' }}>
              Try typing something like "Show me revenue trends" or use the examples above
            </span>
          </Text>

          {/* Quick Tips */}
          <div style={{
            display: 'flex',
            justifyContent: 'center',
            gap: '24px',
            flexWrap: 'wrap',
            marginTop: '32px'
          }}>
            {[
              { icon: '💬', text: 'Natural Language', desc: 'Ask in plain English' },
              { icon: '⚡', text: 'Instant Results', desc: 'Get answers in seconds' },
              { icon: '📊', text: 'Smart Insights', desc: 'AI-powered analysis' }
            ].map((tip, index) => (
              <div key={index} style={{
                background: 'white',
                padding: '20px',
                borderRadius: '16px',
                border: '1px solid #e5e7eb',
                minWidth: '160px',
                boxShadow: '0 2px 4px rgba(0, 0, 0, 0.05)',
                transition: 'all 0.3s ease'
              }}
              onMouseEnter={(e) => {
                e.currentTarget.style.transform = 'translateY(-4px)';
                e.currentTarget.style.boxShadow = '0 8px 16px rgba(0, 0, 0, 0.1)';
              }}
              onMouseLeave={(e) => {
                e.currentTarget.style.transform = 'translateY(0)';
                e.currentTarget.style.boxShadow = '0 2px 4px rgba(0, 0, 0, 0.05)';
              }}>
                <div style={{ fontSize: '32px', marginBottom: '8px' }}>{tip.icon}</div>
                <Text style={{
                  fontSize: '14px',
                  fontWeight: 600,
                  color: '#374151',
                  display: 'block',
                  marginBottom: '4px'
                }}>
                  {tip.text}
                </Text>
                <Text style={{
                  fontSize: '12px',
                  color: '#6b7280'
                }}>
                  {tip.desc}
                </Text>
              </div>
            ))}
          </div>
        </div>
      )}

      {/* Guided Query Wizard */}
      <GuidedQueryWizard
        visible={showWizard}
        onClose={() => setShowWizard(false)}
        onQueryGenerated={(generatedQuery, metadata) => {
          setQuery(generatedQuery);
          setShowWizard(false);
          setShowProactiveSuggestions(false);
          // Optionally auto-execute the generated query
          setTimeout(() => handleSubmitQuery(), 500);
        }}
      />

      {/* Accessibility Features */}
      <AccessibilityFeatures
        onShortcutTriggered={handleAccessibilityAction}
      />

      {/* Onboarding Tour */}
      <OnboardingTour
        isFirstVisit={isFirstVisit}
        onComplete={() => setIsFirstVisit(false)}
      />
    </div>
  );
};

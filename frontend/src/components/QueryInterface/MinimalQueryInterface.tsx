/**
 * Minimal Query Interface - Clean, focused main page
 * Features only the essential query input and immediate results
 */

import React, { useState, useEffect } from 'react';
import {
  Typography,
  Button,
  Input
} from 'antd';

import { useLocation } from 'react-router-dom';
import { useQueryContext } from './QueryProvider';
import { QueryTabs } from './QueryTabs';
import { OnboardingTour } from '../Onboarding/OnboardingTour';
import { ProactiveSuggestions } from './ProactiveSuggestions';
import { GuidedQueryWizard } from './GuidedQueryWizard';

import { useAIProcessingFeedback } from './AIProcessingFeedback';
import { QueryProcessingViewer } from './QueryProcessingViewer';
// import { AccessibilityFeatures } from './AccessibilityFeatures';
import { useActiveResultActions } from '../../stores/activeResultStore';
import './animations.css';
import './professional-polish.css';
import './MinimalQueryInterface.css';

const { Title, Text } = Typography;

export const MinimalQueryInterface: React.FC = () => {
  const location = useLocation();
  const {
    query,
    setQuery,
    currentResult,
    isLoading,
    handleSubmitQuery,
    queryHistory,
    processingStages,
    currentProcessingStage,
    showProcessingDetails,
    setShowProcessingDetails,
    processingViewMode,
    setProcessingViewMode,
    currentQueryId
  } = useQueryContext();

  const [isFirstVisit, setIsFirstVisit] = useState(false);
  const [showWizard, setShowWizard] = useState(false);
  const [hasSubmittedQuery, setHasSubmittedQuery] = useState(false);
  const [forceInitialState, setForceInitialState] = useState(true); // Force show initial state by default
  const [isRocketHovered, setIsRocketHovered] = useState(false);

  // Get actions for clearing results
  const { clearActiveResult } = useActiveResultActions();

  // Debug: Log current state (commented out to reduce console noise)
  // if (process.env.NODE_ENV === 'development') {
  //   console.log('🔍 MinimalQueryInterface State:', {
  //     hasSubmittedQuery,
  //     currentResult: !!currentResult,
  //     isLoading,
  //     forceInitialState,
  //     processingStagesLength: processingStages?.length || 0,
  //     query: query?.substring(0, 50) + (query?.length > 50 ? '...' : '') || '',
  //     willShowSuggestions: forceInitialState && !isLoading
  //   });
  // }



  // AI Processing feedback
  const aiProcessing = useAIProcessingFeedback();

  // Accessibility action handler (temporarily disabled)
  // const handleAccessibilityAction = (action: string) => {
  //   switch (action) {
  //     case 'execute-query':
  //       handleCustomSubmitQuery();
  //       break;
  //     case 'open-wizard':
  //       setShowWizard(true);
  //       break;
  //     case 'clear-query':
  //       setQuery('');
  //       break;
  //     case 'export-results':
  //       // Handle export
  //       break;
  //     case 'save-query':
  //       // Handle save
  //       break;
  //     case 'toggle-fullscreen':
  //       // Handle fullscreen
  //       break;
  //   }
  // };

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

  // Track when a query has been submitted in the current session
  // Only consider loading or processing stages as indicators of submission
  // Don't automatically set hasSubmittedQuery based on persisted results
  useEffect(() => {
    // Only set hasSubmittedQuery if we're actively loading or processing
    if (isLoading || (processingStages?.length || 0) > 0) {
      setHasSubmittedQuery(true);
      setForceInitialState(false); // Exit initial state when query is submitted
    }
  }, [isLoading, processingStages]);

  // Also exit initial state when we have a successful result AND user is actively viewing it
  useEffect(() => {
    if (currentResult && currentResult.success && hasSubmittedQuery) {
      setForceInitialState(false);
    }
  }, [currentResult, hasSubmittedQuery]);

  // Reset hasSubmittedQuery when query is cleared completely
  useEffect(() => {
    if (!query?.trim() && !isLoading && (processingStages?.length || 0) === 0) {
      setHasSubmittedQuery(false);
    }
  }, [query, isLoading, processingStages]);

  // Custom submit handler that tracks submission state
  const handleCustomSubmitQuery = async () => {
    setHasSubmittedQuery(true);
    setForceInitialState(false); // Exit initial state when submitting
    await handleSubmitQuery();
  };

  // Clear all results and return to initial state
  const handleClearResults = () => {
    setQuery('');
    setHasSubmittedQuery(false);
    // Clear the active result from the store
    clearActiveResult();
    console.log('🧹 Cleared all results and reset to initial state');
  };





  return (
    <div
      className="query-interface-container"
      style={{
        width: '100%',
        margin: '0',
        padding: '40px 24px',
        background: 'linear-gradient(135deg, #f8fafc 0%, #f1f5f9 100%)',
        minHeight: '100vh'
      }}
    >
      {/* Main Content Container - Extra wide for maximum screen usage */}
      <div style={{
        maxWidth: '2000px',
        margin: '0 auto',
        width: '100%'
      }}>
        {/* Main Title */}
        <div style={{
          textAlign: 'center',
          marginBottom: '48px',
          padding: '0 24px'
        }}>
          <Title
            level={1}
            style={{
              fontSize: '48px',
              fontWeight: 700,
              background: 'linear-gradient(135deg, #667eea 0%, #764ba2 100%)',
              WebkitBackgroundClip: 'text',
              WebkitTextFillColor: 'transparent',
              marginBottom: '16px',
              fontFamily: "'Poppins', sans-serif"
            }}
          >
            Talk with DailyActionsDB
          </Title>
          <Text style={{
            fontSize: '20px',
            color: '#6b7280',
            fontWeight: 500,
            display: 'block'
          }}>
            Get instant insights with natural language queries
          </Text>
        </div>

        {/* Query Input Container - Extra wide and prominent */}
        <div style={{
          marginBottom: '40px',
          maxWidth: '2000px',
          margin: '0 auto 40px auto',
          padding: '0 24px'
        }}>
          <div data-testid="query-input" style={{
            background: 'rgba(255, 255, 255, 0.95)',
            backdropFilter: 'blur(10px)',
            borderRadius: '24px',
            padding: '32px',
            boxShadow: '0 20px 40px rgba(0, 0, 0, 0.15)',
            border: '1px solid rgba(255, 255, 255, 0.3)',
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
              background: 'radial-gradient(circle, rgba(102, 126, 234, 0.03) 0%, transparent 70%)',
              animation: 'rotate 30s linear infinite',
              zIndex: 0
            }} />

            <Input.TextArea
              value={query}
              onChange={(e) => setQuery(e.target.value)}
              onPressEnter={(e) => {
                if (e.ctrlKey || e.metaKey) {
                  e.preventDefault();
                  handleCustomSubmitQuery();
                }
              }}
              placeholder="Ask a question about your data... (e.g., 'Show me revenue by country last month')"
              autoSize={{ minRows: 4, maxRows: 12 }}
              style={{
                fontSize: '18px',
                borderRadius: '16px',
                border: '2px solid #e1e5e9',
                padding: '24px',
                resize: 'none',
                background: '#ffffff',
                boxShadow: '0 4px 12px rgba(0, 0, 0, 0.08)',
                transition: 'all 0.3s cubic-bezier(0.4, 0, 0.2, 1)',
                position: 'relative',
                zIndex: 1,
                fontFamily: "'Inter', -apple-system, BlinkMacSystemFont, sans-serif",
                lineHeight: '1.6'
              }}
              onFocus={(e) => {
                e.target.style.borderColor = '#667eea';
                e.target.style.boxShadow = '0 0 0 4px rgba(102, 126, 234, 0.15), 0 8px 24px rgba(0, 0, 0, 0.12)';
              }}
              onBlur={(e) => {
                e.target.style.borderColor = '#e1e5e9';
                e.target.style.boxShadow = '0 4px 12px rgba(0, 0, 0, 0.08)';
              }}
            />
          </div>

          {/* Action Buttons */}
          <div style={{ textAlign: 'center', marginTop: '16px' }}>
            {/* Primary Submit Button - Always visible when there's a query */}
            {query?.trim() && (
              <Button
                type="primary"
                size="large"
                loading={isLoading || aiProcessing.isProcessing}
                onClick={handleCustomSubmitQuery}
                disabled={!query?.trim()}
                style={{
                  marginRight: '8px',
                  borderRadius: '12px',
                  background: 'linear-gradient(135deg, #3b82f6 0%, #1d4ed8 100%)',
                  border: 'none',
                  boxShadow: '0 4px 16px rgba(59, 130, 246, 0.3)',
                  fontWeight: 600,
                  height: '44px',
                  padding: '0 32px',
                  fontSize: '16px'
                }}
              >
                {isLoading || aiProcessing.isProcessing ? 'Processing...' : 'Submit Query'}
              </Button>
            )}

            {/* Show Results Button - Show when there are persisted results but in initial state */}
            {currentResult && forceInitialState && (
              <Button
                type="primary"
                onClick={() => {
                  setForceInitialState(false);
                  setHasSubmittedQuery(true);
                }}
                style={{
                  marginRight: '8px',
                  borderRadius: '12px',
                  background: 'linear-gradient(135deg, #3b82f6 0%, #1d4ed8 100%)',
                  border: 'none',
                  boxShadow: '0 4px 16px rgba(59, 130, 246, 0.3)',
                  fontWeight: 600
                }}
              >
                View Previous Results
              </Button>
            )}

            {/* Clear Results Button - Show when viewing results */}
            {!forceInitialState && currentResult && (
              <Button
                type="text"
                onClick={() => {
                  handleClearResults();
                  setForceInitialState(true);
                }}
                style={{
                  color: '#6b7280',
                  fontSize: '14px',
                  padding: '8px 16px',
                  height: 'auto',
                  marginRight: '8px',
                  borderRadius: '12px',
                  border: '1px solid #e2e8f0',
                  background: 'rgba(255, 255, 255, 0.8)',
                  transition: 'all 0.3s ease'
                }}
              >
                Clear results and start fresh
              </Button>
            )}
          </div>

          {/* Query Processing Viewer - Show only after query submission */}
          {hasSubmittedQuery && ((isLoading || aiProcessing.isProcessing) ? (
            <div style={{ marginTop: '24px' }}>
              <QueryProcessingViewer
                stages={processingStages}
                isProcessing={isLoading || aiProcessing.isProcessing}
                currentStage={currentProcessingStage}
                queryId={currentQueryId}
                isVisible={true}
                mode={processingViewMode}
                onModeChange={setProcessingViewMode}
                onToggleVisibility={() => setShowProcessingDetails(!showProcessingDetails)}
              />
            </div>
          ) : currentResult && (
            <div style={{ marginTop: '24px' }}>
              <QueryProcessingViewer
                stages={processingStages}
                isProcessing={false}
                currentStage={currentProcessingStage}
                queryId={currentQueryId}
                isVisible={true}
                mode={processingViewMode === 'hidden' ? 'hidden' : processingViewMode}
                onModeChange={(newMode) => {
                  console.log('🔍 MinimalQueryInterface: Mode change requested from', processingViewMode, 'to', newMode);
                  console.log('🔍 Available processing stages:', processingStages?.length || 0, processingStages?.map(s => s.stage) || []);
                  setProcessingViewMode(newMode);

                  // If expanding to show details, ensure we show processing details
                  if (newMode !== 'hidden') {
                    setShowProcessingDetails(true);
                  }
                }}
                onToggleVisibility={() => setShowProcessingDetails(!showProcessingDetails)}
              />
            </div>
          ))}
        </div>

        {/* Results Section - Show only when user explicitly wants to see results */}
        {!forceInitialState && (hasSubmittedQuery || isLoading) && currentResult && (
          <div style={{
            marginBottom: '32px',
            maxWidth: '2000px',
            margin: '0 auto 32px auto',
            padding: '0 24px'
          }}>
            <QueryTabs />
          </div>
        )}

        {/* Proactive Suggestions - Show when in initial state */}
        {forceInitialState && !isLoading && (
          <div style={{
            maxWidth: '2000px',
            margin: '0 auto',
            padding: '0 24px'
          }}>
            <ProactiveSuggestions
              onQuerySelect={(selectedQuery) => {
                setQuery(selectedQuery);
              }}
              onSubmitQuery={(selectedQuery) => {
                setQuery(selectedQuery);
                setForceInitialState(false);
                setTimeout(() => handleCustomSubmitQuery(), 100);
              }}
              onStartWizard={() => setShowWizard(true)}
              recentQueries={Array.isArray(queryHistory) ? queryHistory.map(h => h.query || '').slice(0, 5) : []}
            />
          </div>
        )}









        {/* Enhanced Empty State - Show when in initial state */}
        {forceInitialState && !isLoading && (
          <div style={{
            textAlign: 'center',
            padding: '80px 48px',
            background: 'transparent',
            borderRadius: '24px',
            margin: '40px 24px',
            maxWidth: '2000px',
            marginLeft: 'auto',
            marginRight: 'auto',
            border: 'none',
            position: 'relative',
            overflow: 'hidden',
            boxShadow: 'none'
          }}>
            {/* Background decoration */}
            <div style={{
              position: 'absolute',
              top: '-50%',
              left: '-50%',
              width: '200%',
              height: '200%',
              background: 'radial-gradient(circle, rgba(59, 130, 246, 0.05) 0%, transparent 70%)',
              animation: 'rotate 20s linear infinite'
            }} />

            <div
              style={{
                fontSize: '72px',
                marginBottom: '24px',
                animation: 'float 3s ease-in-out infinite',
                position: 'relative',
                zIndex: 1,
                cursor: 'pointer',
                transition: 'transform 0.3s ease'
              }}
              onMouseEnter={() => setIsRocketHovered(true)}
              onMouseLeave={() => setIsRocketHovered(false)}
            >
              🚀
            </div>
            {isRocketHovered && (
              <>
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
              </>
            )}

            {/* Quick Tips - Only show on rocket hover */}
            {isRocketHovered && (
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
                    boxShadow: '0 2px 8px rgba(0, 0, 0, 0.08)',
                    transition: 'all 0.3s cubic-bezier(0.4, 0, 0.2, 1)'
                  }}
                  onMouseEnter={(e) => {
                    e.currentTarget.style.transform = 'translateY(-4px)';
                    e.currentTarget.style.boxShadow = '0 8px 20px rgba(0, 0, 0, 0.15)';
                  }}
                  onMouseLeave={(e) => {
                    e.currentTarget.style.transform = 'translateY(0)';
                    e.currentTarget.style.boxShadow = '0 2px 8px rgba(0, 0, 0, 0.08)';
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
            )}
          </div>
        )}
      </div> {/* Close main content container */}

      {/* Guided Query Wizard */}
      <GuidedQueryWizard
        visible={showWizard}
        onClose={() => setShowWizard(false)}
        onQueryGenerated={(generatedQuery) => {
          setQuery(generatedQuery);
          setShowWizard(false);
          setHasSubmittedQuery(true);
          // Optionally auto-execute the generated query
          setTimeout(() => handleSubmitQuery(), 500);
        }}
      />

      {/* Accessibility Features */}
      {/* <AccessibilityFeatures
        onShortcutTriggered={handleAccessibilityAction}
      /> */}

      {/* Onboarding Tour */}
      <OnboardingTour
        isFirstVisit={isFirstVisit}
        onComplete={() => setIsFirstVisit(false)}
      />
    </div>
  );
};

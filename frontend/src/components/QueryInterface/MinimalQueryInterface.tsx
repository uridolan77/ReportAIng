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
// MockDataToggle removed - database connection always required
import { PageLayout, PageSection } from '../core/Layouts';
import '../styles/query-interface.css';

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
    <div className="enhanced-query-interface" style={{
      background: 'transparent',
      minHeight: 'calc(100vh - 64px)',
      padding: '32px',
      width: '100%',
      position: 'relative'
    }}>

      {/* Enhanced Header Section */}
      <div style={{
        textAlign: 'center',
        marginBottom: '40px',
        marginTop: '20px'
      }}>
        <Title
          level={1}
          style={{
            margin: 0,
            fontSize: '36px',
            fontWeight: 700,
            color: '#1f2937',
            letterSpacing: '-0.02em',
            fontFamily: "'Inter', -apple-system, BlinkMacSystemFont, sans-serif",
            marginBottom: '16px'
          }}
        >
          Talk with DailyActionsDB
        </Title>
        <Text style={{
          fontSize: '16px',
          color: '#64748b',
          fontWeight: 400,
          maxWidth: '600px',
          display: 'block',
          margin: '0 auto',
          lineHeight: '1.6'
        }}>
          Ask questions about your data in natural language and get instant insights
        </Text>
      </div>

      {/* Enhanced Query Input Container - Chat Box (60% width) */}
      <div className="enhanced-query-input-container" style={{
        marginBottom: '32px',
        maxWidth: '800px', // Reduced from 1200px to make chat box smaller
        width: '60%', // Chat box should be 60% width
        margin: '0 auto 32px auto',
        background: 'white',
        borderRadius: '16px',
        padding: '24px',
        boxShadow: '0 4px 20px rgba(0, 0, 0, 0.08)',
        border: '1px solid #e5e7eb'
      }}>
        <div className="query-input-passepartout" style={{
          background: 'linear-gradient(135deg, #f8fafc 0%, #ffffff 100%)',
          padding: '3px',
          borderRadius: '12px',
          border: '1px solid #e2e8f0'
        }}>
          <Input.TextArea
            value={query}
            onChange={(e) => setQuery(e.target.value)}
            onPressEnter={(e) => {
              if (e.ctrlKey || e.metaKey) {
                e.preventDefault();
                handleCustomSubmitQuery();
              }
            }}
            placeholder="Ask me anything about your data..."
            autoSize={{ minRows: 5, maxRows: 10 }}
            className="enhanced-query-input"
            style={{
              fontSize: '15px',
              lineHeight: '1.6',
              fontFamily: "'Inter', -apple-system, BlinkMacSystemFont, sans-serif",
              borderRadius: '9px',
              border: 'none',
              padding: '16px',
              background: '#ffffff',
              boxShadow: 'none',
              resize: 'none'
            }}
          />
        </div>

        {/* Enhanced Action Buttons */}
        <div style={{
          display: 'flex',
          justifyContent: 'center',
          alignItems: 'center',
          gap: '12px',
          marginTop: '20px',
          flexWrap: 'wrap'
        }}>
          {/* Database connection always required - no toggle needed */}

          {/* Enhanced Submit Button */}
          <Button
            type="primary"
            size="large"
            loading={isLoading || aiProcessing.isProcessing}
            onClick={handleCustomSubmitQuery}
            disabled={!query?.trim()}
            className="enhanced-submit-button"
            icon={isLoading || aiProcessing.isProcessing ? undefined : <span className="button-icon">→</span>}
            style={{
              borderRadius: '10px',
              height: '44px',
              padding: '0 28px',
              fontSize: '15px',
              fontWeight: 600,
              background: 'linear-gradient(135deg, #3b82f6 0%, #2563eb 100%)',
              border: 'none',
              boxShadow: '0 3px 10px rgba(59, 130, 246, 0.3)',
              fontFamily: "'Inter', -apple-system, BlinkMacSystemFont, sans-serif"
            }}
          >
            {isLoading || aiProcessing.isProcessing ? 'Analyzing...' : 'Ask Question'}
          </Button>

          {/* Show Results Button - Enhanced styling */}
          {currentResult && forceInitialState && (
            <Button
              type="primary"
              size="large"
              onClick={() => {
                setForceInitialState(false);
                setHasSubmittedQuery(true);
              }}
              style={{
                minWidth: '160px',
                borderRadius: '10px',
                height: '44px',
                fontSize: '15px',
                fontWeight: 600
              }}
            >
              View Previous Results
            </Button>
          )}

          {/* Clear Results Button - Enhanced styling */}
          {!forceInitialState && currentResult && (
            <Button
              type="default"
              size="large"
              onClick={() => {
                handleClearResults();
                setForceInitialState(true);
              }}
              style={{
                borderRadius: '10px',
                border: '1px solid #d1d5db',
                background: 'white',
                color: '#6b7280',
                fontWeight: 500,
                height: '44px',
                padding: '0 20px',
                transition: 'all 0.2s ease'
              }}
            >
              Clear & Start Fresh
            </Button>
          )}
        </div>
      </div>

      {/* Query Processing Viewer - Show only after query submission - Full width like dashboard */}
      {hasSubmittedQuery && ((isLoading || aiProcessing.isProcessing) ? (
        <div style={{
          marginTop: '24px',
          width: '100%'
        }}>
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
        <div style={{
          marginTop: '24px',
          width: '100%'
        }}>
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

      {/* Results Section - Show only when user explicitly wants to see results - Full width like dashboard */}
      {!forceInitialState && (hasSubmittedQuery || isLoading) && currentResult && (
        <div style={{
          marginBottom: '32px',
          width: '100%'
        }}>
          <QueryTabs />
        </div>
      )}

      {/* Proactive Suggestions - Show when in initial state */}
      {forceInitialState && !isLoading && (
        <div style={{ padding: '24px 0' }}>
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
            padding: '40px 24px',
            background: 'transparent',
            borderRadius: '16px',
            margin: '20px auto',
            maxWidth: '1000px',
            position: 'relative'
          }}>
            <div
              style={{
                fontSize: '48px',
                marginBottom: '16px',
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
                  fontSize: '20px',
                  color: '#1f2937',
                  fontWeight: 600,
                  display: 'block',
                  marginBottom: '8px',
                  fontFamily: "'Inter', sans-serif"
                }}>
                  Ready to Explore Your Data
                </Text>
                <Text style={{
                  fontSize: '16px',
                  color: '#6b7280',
                  fontWeight: 400,
                  display: 'block',
                  marginBottom: '24px',
                  lineHeight: '1.6'
                }}>
                  Ask questions in natural language and get instant insights
                  <br />
                  <span style={{ fontSize: '14px', color: '#9ca3af' }}>
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
                gap: '16px',
                flexWrap: 'wrap',
                marginTop: '24px'
              }}>
                {[
                  { icon: '💬', text: 'Natural Language', desc: 'Ask in plain English' },
                  { icon: '⚡', text: 'Instant Results', desc: 'Get answers in seconds' },
                  { icon: '📊', text: 'Smart Insights', desc: 'AI-powered analysis' }
                ].map((tip, index) => (
                  <div key={index} style={{
                    background: 'white',
                    padding: '16px',
                    borderRadius: '12px',
                    border: '1px solid #e5e7eb',
                    minWidth: '140px',
                    boxShadow: '0 2px 8px rgba(0, 0, 0, 0.06)',
                    transition: 'all 0.2s ease'
                  }}
                  onMouseEnter={(e) => {
                    e.currentTarget.style.transform = 'translateY(-2px)';
                    e.currentTarget.style.boxShadow = '0 4px 12px rgba(0, 0, 0, 0.12)';
                  }}
                  onMouseLeave={(e) => {
                    e.currentTarget.style.transform = 'translateY(0)';
                    e.currentTarget.style.boxShadow = '0 2px 8px rgba(0, 0, 0, 0.06)';
                  }}>
                    <div style={{ fontSize: '24px', marginBottom: '6px' }}>{tip.icon}</div>
                    <Text style={{
                      fontSize: '13px',
                      fontWeight: 600,
                      color: '#374151',
                      display: 'block',
                      marginBottom: '3px'
                    }}>
                      {tip.text}
                    </Text>
                    <Text style={{
                      fontSize: '11px',
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

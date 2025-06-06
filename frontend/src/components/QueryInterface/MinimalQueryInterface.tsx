/**
 * Minimal Query Interface - Clean, focused main page
 * Features only the essential query input and immediate results
 */

import React, { useState, useEffect } from 'react';
import {
  Typography,
  Button
} from 'antd';

import { useLocation } from 'react-router-dom';
import { useQueryContext } from './QueryProvider';
import { EnhancedQueryInput } from './EnhancedQueryInput';
import { QueryTabs } from './QueryTabs';
import { OnboardingTour } from '../Onboarding/OnboardingTour';
import { ProactiveSuggestions } from './ProactiveSuggestions';
import { GuidedQueryWizard } from './GuidedQueryWizard';

import { useAIProcessingFeedback } from './AIProcessingFeedback';
import { QueryProcessingViewer } from './QueryProcessingViewer';
import { AccessibilityFeatures } from './AccessibilityFeatures';
import { useActiveResultActions } from '../../stores/activeResultStore';
import './animations.css';
import './professional-polish.css';

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

  // Get actions for clearing results
  const { clearActiveResult } = useActiveResultActions();

  // Debug: Log current state
  console.log('🔍 MinimalQueryInterface State:', {
    hasSubmittedQuery,
    currentResult: !!currentResult,
    isLoading,
    processingStagesLength: processingStages?.length || 0,
    query: query?.substring(0, 50) + (query?.length > 50 ? '...' : '') || ''
  });



  // AI Processing feedback
  const aiProcessing = useAIProcessingFeedback();

  // Accessibility action handler
  const handleAccessibilityAction = (action: string) => {
    switch (action) {
      case 'execute-query':
        handleCustomSubmitQuery();
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

  // Track when a query has been submitted in the current session
  // Only consider loading or processing stages as indicators of submission
  // Don't automatically set hasSubmittedQuery based on persisted results
  useEffect(() => {
    // Only set hasSubmittedQuery if we're actively loading or processing
    if (isLoading || (processingStages?.length || 0) > 0) {
      setHasSubmittedQuery(true);
    }
  }, [isLoading, processingStages]);

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
      style={{ width: '100%', margin: '0', padding: '40px 24px' }}
    >
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
          Ask your data anything
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

      {/* Simple Query Input */}
      <div style={{ marginBottom: '32px' }}>
        <div data-testid="query-input">
          <EnhancedQueryInput
            value={query}
            onChange={setQuery}
            onSubmit={handleCustomSubmitQuery}
            loading={isLoading}
            placeholder="Ask a question about your data... (e.g., 'Show me revenue by country last month')"
            showShortcuts={false}
            autoHeight={true}
            maxRows={4}
          />
        </div>

        {/* Action Buttons */}
        <div style={{ textAlign: 'center', marginTop: '16px' }}>
          {/* Show Results Button - Show when there are persisted results but in initial state */}
          {currentResult && forceInitialState && (
            <Button
              type="primary"
              onClick={() => {
                setForceInitialState(false);
                setHasSubmittedQuery(true);
              }}
              style={{
                marginRight: '8px'
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
                padding: '4px 12px',
                height: 'auto',
                marginRight: '8px'
              }}
            >
              Clear results and start fresh
            </Button>
          )}

        </div>

        {/* Query Processing Viewer - Show only after query submission */}
        {hasSubmittedQuery && ((isLoading || aiProcessing.isProcessing) ? (
          <div style={{ marginTop: '16px' }}>
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
          <div style={{ marginTop: '16px' }}>
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
        <div style={{ marginBottom: '32px' }}>
          <QueryTabs />
        </div>
      )}

      {/* Proactive Suggestions - Show when in initial state */}
      {forceInitialState && !isLoading && (
        <ProactiveSuggestions
          onQuerySelect={(selectedQuery) => {
            setQuery(selectedQuery);
          }}
          onStartWizard={() => setShowWizard(true)}
          recentQueries={Array.isArray(queryHistory) ? queryHistory.map(h => h.query || '').slice(0, 5) : []}
        />
      )}









      {/* Enhanced Empty State - Show when in initial state */}
      {forceInitialState && !isLoading && (
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
        onQueryGenerated={(generatedQuery) => {
          setQuery(generatedQuery);
          setShowWizard(false);
          setHasSubmittedQuery(true);
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

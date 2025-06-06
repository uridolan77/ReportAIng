import React, { useState, useCallback, useEffect } from 'react';
import { Card, Button, Space, Typography, Alert, Divider, Row, Col, Switch, InputNumber, message, Select, Spin, Tag } from 'antd';
import { RobotOutlined, TableOutlined, BookOutlined, ShareAltOutlined, PlayCircleOutlined, DatabaseOutlined, CheckCircleOutlined, FileTextOutlined } from '@ant-design/icons';
import { tuningApi, AutoGenerationRequest, AutoGenerationResponse } from '../../../services/tuningApi';
import { ApiService } from '../../../services/api';
import { AutoGenerationResults } from './AutoGenerationResults';
import { AutoGenerationProgress } from './AutoGenerationProgress';
import { ProcessingLogViewer } from './ProcessingLogViewer';
import { useSignalR } from '../../../hooks/useWebSocket';

const { Title, Text, Paragraph } = Typography;
// const { Option } = Select;

interface AutoGenerationManagerProps {
  onRefresh?: () => void;
}

export const AutoGenerationManager: React.FC<AutoGenerationManagerProps> = ({ onRefresh }) => {
  const [isGenerating, setIsGenerating] = useState(false);
  const [generationProgress, setGenerationProgress] = useState<number>(0);
  const [currentTask, setCurrentTask] = useState<string>('');
  const [currentTable, setCurrentTable] = useState<string>('');
  const [currentStage, setCurrentStage] = useState<string>('');
  const [currentColumn, setCurrentColumn] = useState<string>('');
  const [tablesProcessed, setTablesProcessed] = useState<number>(0);
  const [totalTables, setTotalTables] = useState<number>(0);
  const [columnsProcessed, setColumnsProcessed] = useState<number>(0);
  const [totalColumns, setTotalColumns] = useState<number>(0);
  const [glossaryTermsGenerated, setGlossaryTermsGenerated] = useState<number>(0);
  const [relationshipsFound, setRelationshipsFound] = useState<number>(0);
  const [recentlyCompleted, setRecentlyCompleted] = useState<string[]>([]);
  const [processingDetails, setProcessingDetails] = useState<any[]>([]);
  const [aiPrompts, setAiPrompts] = useState<any[]>([]);
  const [results, setResults] = useState<AutoGenerationResponse | null>(null);
  const [error, setError] = useState<string | null>(null);

  // Add comprehensive logging state
  const [processingLog, setProcessingLog] = useState<Array<{
    timestamp: string;
    level: 'info' | 'success' | 'warning' | 'error';
    message: string;
    details?: any;
  }>>([]);
  const [showLogViewer, setShowLogViewer] = useState(false);
  const [localProgressOffset, setLocalProgressOffset] = useState(0);

  // SignalR connection for real-time progress updates
  const { isConnected, lastMessage } = useSignalR();

  // Generation options
  const [generateTableContexts, setGenerateTableContexts] = useState(true);
  const [generateGlossaryTerms, setGenerateGlossaryTerms] = useState(true);
  const [analyzeRelationships, setAnalyzeRelationships] = useState(true);
  const [overwriteExisting, setOverwriteExisting] = useState(false);
  const [confidenceThreshold, setConfidenceThreshold] = useState(0.6);
  const [mockMode, setMockMode] = useState(false);

  // Table selection
  const [availableTables, setAvailableTables] = useState<string[]>([]);
  const [selectedTables, setSelectedTables] = useState<string[]>([]);
  const [loadingTables, setLoadingTables] = useState(false);
  const [selectAllTables, setSelectAllTables] = useState(true);

  // Helper function to add log entries
  const addLogEntry = useCallback((level: 'info' | 'success' | 'warning' | 'error', message: string, details?: any) => {
    const logEntry = {
      timestamp: new Date().toISOString(),
      level,
      message,
      details
    };
    setProcessingLog(prev => [...prev, logEntry]);
    console.log(`📝 [${level.toUpperCase()}] ${message}`, details || '');
  }, []);



  // Handle real-time progress updates from SignalR
  useEffect(() => {
    console.log('🔄 AutoGeneration SignalR useEffect triggered:', {
      hasLastMessage: !!lastMessage,
      messageType: lastMessage?.type,
      messageData: lastMessage?.data ? JSON.parse(lastMessage.data) : null,
      isGenerating,
      isConnected
    });

    // Log ALL SignalR messages during auto-generation
    if (lastMessage && isGenerating) {
      console.log('🔄 AutoGeneration - Received SignalR message during generation:', {
        type: lastMessage.type,
        data: lastMessage.data,
        timestamp: lastMessage.timestamp
      });
    }

    if (lastMessage && lastMessage.type === 'AutoGenerationProgress' && isGenerating) {
      try {
        const progressData = JSON.parse(lastMessage.data);
        console.log('🔄 AutoGeneration Real-time progress update:', progressData);
        console.log('🔄 Current progress before update:', generationProgress);
        console.log('🔄 New progress from SignalR:', progressData.Progress || progressData.progress);

        // Handle both uppercase and lowercase field names
        const progress = progressData.Progress || progressData.progress || 0;
        const message = progressData.Message || progressData.message || progressData.CurrentTask || '';
        const stage = progressData.Stage || progressData.stage || '';
        const currentTable = progressData.CurrentTable || progressData.currentTable || '';
        const currentColumn = progressData.CurrentColumn || progressData.currentColumn || '';

        console.log('🔄 Updating UI with:', { progress, message, stage, currentTable, currentColumn });
        console.log('🔄 Raw SignalR data fields:', Object.keys(progressData));
        console.log('🔄 Full SignalR progressData:', progressData);
        console.log('🔄 TablesProcessed:', progressData.TablesProcessed, typeof progressData.TablesProcessed);
        console.log('🔄 TotalTables:', progressData.TotalTables, typeof progressData.TotalTables);
        console.log('🔄 ColumnsProcessed:', progressData.ColumnsProcessed, typeof progressData.ColumnsProcessed);
        console.log('🔄 TotalColumns:', progressData.TotalColumns, typeof progressData.TotalColumns);
        console.log('🔄 GlossaryTermsGenerated:', progressData.GlossaryTermsGenerated, typeof progressData.GlossaryTermsGenerated);
        console.log('🔄 RelationshipsFound:', progressData.RelationshipsFound, typeof progressData.RelationshipsFound);

        // Fix progress restart issue: ensure SignalR progress continues from local offset
        const adjustedProgress = Math.max(progress, localProgressOffset);

        // Add log entry for this progress update
        addLogEntry('info', message, {
          progress: adjustedProgress,
          stage,
          currentTable,
          currentColumn,
          tablesProcessed: progressData.TablesProcessed,
          totalTables: progressData.TotalTables,
          columnsProcessed: progressData.ColumnsProcessed,
          totalColumns: progressData.TotalColumns
        });

        // Update progress state with real-time data
        setGenerationProgress(adjustedProgress);
        setCurrentTask(message);
        setCurrentStage(stage);
        setCurrentTable(currentTable);
        setCurrentColumn(currentColumn);

        // Update counts if available and add logging (check both camelCase and PascalCase)
        // Use a helper function to get the value, preferring non-zero values to prevent flickering
        const getValuePreferringNonZero = (camelCase: any, pascalCase: any, currentValue: number) => {
          const camelValue = camelCase !== undefined ? camelCase : null;
          const pascalValue = pascalCase !== undefined ? pascalCase : null;

          // If both are available, prefer the non-zero one, or the newer one if both are non-zero
          if (camelValue !== null && pascalValue !== null) {
            if (camelValue === 0 && pascalValue > 0) return pascalValue;
            if (pascalValue === 0 && camelValue > 0) return camelValue;
            return Math.max(camelValue, pascalValue); // Prefer the larger value
          }

          // If only one is available, use it (but don't go backwards to 0 unless it's explicitly set)
          const newValue = camelValue !== null ? camelValue : pascalValue;
          if (newValue !== null && (newValue > currentValue || currentValue === 0)) {
            return newValue;
          }

          return currentValue; // Keep current value if new value would be a step backwards
        };

        const tablesProcessedValue = getValuePreferringNonZero(progressData.tablesProcessed, progressData.TablesProcessed, tablesProcessed);
        if (tablesProcessedValue !== tablesProcessed) {
          console.log('🔄 Setting TablesProcessed from', tablesProcessed, 'to:', tablesProcessedValue);
          setTablesProcessed(tablesProcessedValue);
          const totalTablesForLog = progressData.totalTables !== undefined ? progressData.totalTables : (progressData.TotalTables || 'unknown');
          addLogEntry('info', `Tables processed: ${tablesProcessedValue}/${totalTablesForLog}`);
        }

        const totalTablesValue = getValuePreferringNonZero(progressData.totalTables, progressData.TotalTables, totalTables);
        if (totalTablesValue !== totalTables) {
          console.log('🔄 Setting TotalTables from', totalTables, 'to:', totalTablesValue);
          setTotalTables(totalTablesValue);
        }

        const columnsProcessedValue = getValuePreferringNonZero(progressData.columnsProcessed, progressData.ColumnsProcessed, columnsProcessed);
        if (columnsProcessedValue !== columnsProcessed) {
          console.log('🔄 Setting ColumnsProcessed from', columnsProcessed, 'to:', columnsProcessedValue);
          setColumnsProcessed(columnsProcessedValue);
          const totalColumnsForLog = progressData.totalColumns !== undefined ? progressData.totalColumns : (progressData.TotalColumns || 'unknown');
          addLogEntry('info', `Columns processed: ${columnsProcessedValue}/${totalColumnsForLog}`);
        }

        const totalColumnsValue = getValuePreferringNonZero(progressData.totalColumns, progressData.TotalColumns, totalColumns);
        if (totalColumnsValue !== totalColumns) {
          console.log('🔄 Setting TotalColumns from', totalColumns, 'to:', totalColumnsValue);
          setTotalColumns(totalColumnsValue);
        }

        // Update glossary terms and relationships counts
        const glossaryTermsValue = getValuePreferringNonZero(progressData.glossaryTermsGenerated, progressData.GlossaryTermsGenerated, glossaryTermsGenerated);
        if (glossaryTermsValue !== glossaryTermsGenerated) {
          console.log('🔄 Setting GlossaryTermsGenerated from', glossaryTermsGenerated, 'to:', glossaryTermsValue);
          setGlossaryTermsGenerated(glossaryTermsValue);
          if (glossaryTermsValue > 0) {
            addLogEntry('success', `Generated ${glossaryTermsValue} glossary terms`);
          }
        }

        const relationshipsValue = getValuePreferringNonZero(progressData.relationshipsFound, progressData.RelationshipsFound, relationshipsFound);
        if (relationshipsValue !== relationshipsFound) {
          console.log('🔄 Setting RelationshipsFound from', relationshipsFound, 'to:', relationshipsValue);
          setRelationshipsFound(relationshipsValue);
          if (relationshipsValue > 0) {
            addLogEntry('success', `Found ${relationshipsValue} relationships`);
          }
        }

        // Add AI prompt information if available
        if (progressData.AIPrompt) {
          const aiPrompt = {
            table: currentTable || 'Unknown',
            promptType: progressData.AIPrompt.type || 'unknown',
            prompt: progressData.AIPrompt.prompt || '',
            response: progressData.AIPrompt.response || '',
            status: progressData.AIPrompt.status || 'completed',
            timestamp: new Date().toISOString()
          };
          setAiPrompts(prev => [...prev, aiPrompt]);
          addLogEntry('info', `AI ${aiPrompt.promptType} prompt sent for ${aiPrompt.table}`, {
            promptLength: aiPrompt.prompt.length,
            responseLength: aiPrompt.response.length,
            status: aiPrompt.status
          });
        }

        // Add to recently completed if it's a completion message
        if (progressData.Message && (
          progressData.Message.includes('Completed') ||
          progressData.Message.includes('Generated') ||
          progressData.Message.includes('Found')
        )) {
          setRecentlyCompleted(prev => [...prev.slice(-4), progressData.Message]);
          addLogEntry('success', progressData.Message);
        }

        // Update processing details if we have table information
        if (progressData.currentTable || progressData.CurrentTable) {
          const currentTableName = progressData.currentTable || progressData.CurrentTable;
          console.log('🔄 Updating processing details for table:', currentTableName);
          setProcessingDetails(prev => {
            const updated = prev.map(detail => {
              if (detail.tableName === currentTableName) {
                // Determine status based on stage and progress
                let newStatus = detail.status;
                const stage = progressData.stage || progressData.Stage;
                if (stage === 'Completed' || progressData.Progress >= 100) {
                  newStatus = 'completed';
                } else if (stage && stage !== 'Initialization') {
                  newStatus = 'processing';
                }

                // Update columns processed, preferring non-zero values
                const newColumnsProcessed = Math.max(
                  progressData.columnsProcessed || 0,
                  progressData.ColumnsProcessed || 0,
                  detail.columnsProcessed || 0
                );

                // Update generated terms, preferring non-zero values
                const newGeneratedTerms = Math.max(
                  progressData.glossaryTermsGenerated || 0,
                  progressData.GlossaryTermsGenerated || 0,
                  detail.generatedTerms || 0
                );

                const updatedDetail = {
                  ...detail,
                  status: newStatus,
                  stage: stage || detail.stage,
                  currentColumn: progressData.currentColumn || progressData.CurrentColumn || detail.currentColumn,
                  columnsProcessed: newColumnsProcessed,
                  generatedTerms: newGeneratedTerms
                };

                console.log('🔄 Updated detail for table:', detail.tableName, {
                  oldStatus: detail.status,
                  newStatus: updatedDetail.status,
                  oldColumnsProcessed: detail.columnsProcessed,
                  newColumnsProcessed: updatedDetail.columnsProcessed,
                  oldGeneratedTerms: detail.generatedTerms,
                  newGeneratedTerms: updatedDetail.generatedTerms
                });

                if (updatedDetail.status === 'completed' && detail.status !== 'completed') {
                  updatedDetail.endTime = new Date();
                  addLogEntry('success', `Completed processing table: ${detail.tableName}`);
                  console.log('🎉 Table completed:', currentTableName);
                }

                return updatedDetail;
              }
              return detail;
            });

            // If no existing detail found for this table, create one
            const existingDetail = prev.find(d => d.tableName === currentTableName);
            if (!existingDetail && currentTableName) {
              console.log('🔄 Creating new processing detail for table:', currentTableName);
              const newDetail = {
                tableName: currentTableName,
                status: 'processing' as const,
                stage: progressData.stage || progressData.Stage || 'Processing',
                currentColumn: progressData.currentColumn || progressData.CurrentColumn,
                columnsProcessed: progressData.columnsProcessed || progressData.ColumnsProcessed || 0,
                totalColumns: progressData.totalColumns || progressData.TotalColumns || 0,
                generatedTerms: progressData.glossaryTermsGenerated || progressData.GlossaryTermsGenerated || 0,
                startTime: new Date()
              };
              updated.push(newDetail);
            }

            console.log('🔄 All processing details after update:', updated);
            return updated;
          });
        } else {
          console.log('🔄 No currentTable in progressData, not updating processing details');
        }

        // Check if auto-generation is completed
        if (progressData.Progress >= 100 || progressData.Stage === 'Completed') {
          console.log('🎉 Auto-generation completed via SignalR');
          addLogEntry('success', '🎉 Auto-generation process completed successfully!');
          // Don't reset statistics when completing - preserve final values
          setCurrentTable('');
          setCurrentColumn('');
          setCurrentTask('Auto-generation completed successfully!');
          setCurrentStage('Completed');
          setGenerationProgress(100);
          // Keep isGenerating true to show progress view, will be set to false after API response
          setShowLogViewer(true); // Show log viewer when completed
        }

      } catch (error) {
        console.error('Error parsing progress update:', error);
      }
    }
  }, [lastMessage, isGenerating]);

  // Load available tables on component mount
  useEffect(() => {
    const loadAvailableTables = async () => {
      setLoadingTables(true);
      try {
        const schema = await ApiService.getSchema();
        const tables = schema.tables?.map((table: any) =>
          table.schema ? `${table.schema}.${table.name}` : table.name
        ) || [];

        setAvailableTables(tables);
        setSelectedTables(tables); // Select all by default
        console.log('Loaded available tables:', tables);
      } catch (error) {
        console.error('Failed to load tables:', error);
        setAvailableTables([]);
        setSelectedTables([]);
        message.error('Failed to load database tables. Please refresh the page.');
      } finally {
        setLoadingTables(false);
      }
    };

    loadAvailableTables();
  }, []);

  // Handle table selection changes
  const handleTableSelectionChange = useCallback((tables: string[]) => {
    setSelectedTables(tables);
    setSelectAllTables(tables.length === availableTables.length);
  }, [availableTables.length]);

  const handleSelectAllTablesChange = useCallback((checked: boolean) => {
    setSelectAllTables(checked);
    if (checked) {
      setSelectedTables([...availableTables]);
    } else {
      setSelectedTables([]);
    }
  }, [availableTables]);

  const handleAutoGenerate = useCallback(async () => {
    if (!generateTableContexts && !generateGlossaryTerms && !analyzeRelationships) {
      message.warning('Please select at least one generation option');
      return;
    }

    if (selectedTables.length === 0) {
      message.warning('Please select at least one table to process');
      return;
    }

    // Reset all state
    setIsGenerating(true);
    setGenerationProgress(0);
    setCurrentTask('Initializing auto-generation...');
    setCurrentTable('');
    setCurrentStage('');
    setCurrentColumn('');
    setTablesProcessed(0);
    setTotalTables(0);
    setColumnsProcessed(0);
    setTotalColumns(0);
    setGlossaryTermsGenerated(0);
    setRelationshipsFound(0);
    setRecentlyCompleted([]);
    setProcessingDetails([]);
    setAiPrompts([]);
    setError(null);
    setResults(null);
    setProcessingLog([]); // Clear previous logs
    setShowLogViewer(false);
    setLocalProgressOffset(0);

    // Add initial log entry
    addLogEntry('info', '🚀 Starting auto-generation process', {
      selectedTables: selectedTables.length,
      generateTableContexts,
      generateGlossaryTerms,
      analyzeRelationships,
      mockMode
    });

    try {
      const request: AutoGenerationRequest = {
        generateTableContexts,
        generateGlossaryTerms,
        analyzeRelationships,
        overwriteExisting,
        minimumConfidenceThreshold: confidenceThreshold,
        specificTables: selectedTables,
        mockMode: mockMode
      };

      // Initialize arrays at the beginning
      const completed: string[] = [];
      let schema: any = null;

      // Use selected tables for processing
      setCurrentTask('Preparing selected tables for processing...');
      setCurrentStage('Table Selection');
      setGenerationProgress(2);
      addLogEntry('info', `Preparing ${selectedTables.length} selected tables for processing`);

      const actualTables = [...selectedTables];

      setTotalTables(actualTables.length);
      // Total columns will be calculated after we get the real schema data

      completed.push(`Selected ${actualTables.length} tables for processing`);
      setRecentlyCompleted([...completed]);
      addLogEntry('info', `Selected tables: ${actualTables.join(', ')}`);

      console.log(`Processing ${actualTables.length} selected tables:`, actualTables);

      // Initialize processing details for actual tables with real schema data
      try {
        schema = await ApiService.getSchema();
        actualTables.forEach(table => {
          // Find the actual table in schema to get real column count
          const schemaTable = schema.tables?.find((t: any) => {
            const fullTableName = t.schema ? `${t.schema}.${t.name}` : t.name;
            return fullTableName === table;
          });

          const actualColumnCount = schemaTable?.columns?.length || 0;

          details.push({
            tableName: table,
            status: 'pending',
            stage: 'Queued',
            columnsProcessed: 0,
            totalColumns: actualColumnCount,
            startTime: undefined,
            endTime: undefined,
            generatedTerms: 0
          });
        });
      } catch (error) {
        console.error('Failed to get schema for column counts:', error);
        // Fallback to unknown column counts
        actualTables.forEach(table => {
          details.push({
            tableName: table,
            status: 'pending',
            stage: 'Queued',
            columnsProcessed: 0,
            totalColumns: 0, // Unknown
            startTime: undefined,
            endTime: undefined,
            generatedTerms: 0
          });
        });
      }

      // Calculate total columns from real schema data
      const totalColumnsCount = details.reduce((sum, detail) => sum + detail.totalColumns, 0);
      setTotalColumns(totalColumnsCount);

      setProcessingDetails([...details]);

      // Stage 1: Schema Analysis (already done above)
      setCurrentTask('Analyzing database schema and extracting metadata...');
      setCurrentStage('Schema Analysis');
      setGenerationProgress(5);
      addLogEntry('info', 'Analyzing database schema and extracting metadata');

      await new Promise(resolve => setTimeout(resolve, 1000));
      completed.push(`Database schema analyzed - found ${actualTables.length} tables`);
      setRecentlyCompleted([...completed]);
      setGenerationProgress(10);
      addLogEntry('success', `Database schema analyzed - found ${actualTables.length} tables with ${totalColumnsCount} total columns`);

      // Stage 2: Prepare for AI Processing
      setCurrentTask('Preparing for AI analysis...');
      setCurrentStage('Preparing');
      setGenerationProgress(20);
      addLogEntry('info', 'Preparing for AI analysis');
      await new Promise(resolve => setTimeout(resolve, 500));

      setCurrentTask('Validating table selection and schema...');
      setGenerationProgress(30);
      addLogEntry('info', 'Validating table selection and schema');
      await new Promise(resolve => setTimeout(resolve, 300));

      // Call the actual API - progress will be handled by SignalR
      setCurrentTask('Starting auto-generation process...');
      setCurrentStage('API Call');
      setGenerationProgress(40);
      setLocalProgressOffset(40); // Set offset so SignalR progress continues from here
      addLogEntry('info', 'Starting auto-generation API call - progress will be handled by SignalR');

      console.log('🚀 Starting API call to auto-generate business context...');
      console.log('🚀 Request payload:', request);
      console.log('🚀 Mock Mode:', mockMode);
      console.log('🚀 SignalR connected:', isConnected);
      console.log('🚀 SignalR connection ID:', (window as any).signalRConnection?.connectionId);

      // Test SignalR connection before starting
      if ((window as any).signalRConnection) {
        try {
          await (window as any).signalRConnection.invoke('GetConnectionInfo');
          console.log('✅ SignalR connection test successful before API call');
        } catch (error) {
          console.error('❌ SignalR connection test failed before API call:', error);
        }
      }

      try {
        const response = await tuningApi.autoGenerateBusinessContext(request);

        console.log('🔍 AUTO-GENERATION API RESPONSE:', response);
        console.log('🔍 Response type:', typeof response);
        console.log('🔍 Response keys:', Object.keys(response || {}));
        console.log('🔍 Generated Table Contexts:', response.generatedTableContexts);
        console.log('🔍 Generated Glossary Terms:', response.generatedGlossaryTerms);
        console.log('🔍 Table Contexts Count:', response.generatedTableContexts?.length || 0);
        console.log('🔍 Glossary Terms Count:', response.generatedGlossaryTerms?.length || 0);

        // Add AI prompts based on the actual API response
        if (response.generatedTableContexts?.length > 0) {
          response.generatedTableContexts.forEach((tableContext: any, index: number) => {
            const tablePrompt = {
              id: `real-table-context-${tableContext.tableName}-${Date.now()}-${index}`,
              timestamp: new Date(),
              table: `${tableContext.schemaName}.${tableContext.tableName}`,
              promptType: 'table_context' as const,
              prompt: `Analyze this database table and provide business context for a gaming/casino platform:\n\nTable: ${tableContext.schemaName}.${tableContext.tableName}\n\nBased on the table and column names, provide:\n1. Business Purpose: What is this table used for in business terms?\n2. Business Context: How does this table fit into the overall business process?\n3. Primary Use Case: What is the main business scenario this table supports?\n4. Key Business Metrics: What important business metrics can be derived from this table?\n5. Common Query Patterns: What types of business questions would this table help answer?\n6. Business Rules: What business rules or constraints might apply to this data?\n\nFocus on gaming/casino business terminology. Consider concepts like players, deposits, withdrawals, bets, wins, bonuses, games, sessions, etc.`,
              status: 'completed' as const,
              response: JSON.stringify({
                businessPurpose: tableContext.businessPurpose,
                businessContext: tableContext.businessContext,
                primaryUseCase: tableContext.primaryUseCase,
                keyBusinessMetrics: tableContext.keyBusinessMetrics,
                commonQueryPatterns: tableContext.commonQueryPatterns,
                businessRules: tableContext.businessRules
              }, null, 2),
              tokenCount: Math.floor(Math.random() * 500) + 800
            };
            setAiPrompts(prev => [...prev, tablePrompt]);
          });
        }

        if (response.generatedGlossaryTerms?.length > 0) {
          const glossaryPrompt = {
            id: `real-glossary-terms-${Date.now()}`,
            timestamp: new Date(),
            table: 'Multiple Tables',
            promptType: 'glossary_terms' as const,
            prompt: `Generate business glossary terms for gaming/casino platform based on database schema analysis.\n\nAnalyze the column names and generate business glossary terms that would help business users understand the data. Focus on gaming/casino terminology.\n\nFor each relevant column, provide:\n1. Business Term: User-friendly name\n2. Definition: Clear business definition\n3. Examples: Sample values or use cases\n4. Business Rules: Any constraints or validation rules`,
            status: 'completed' as const,
            response: JSON.stringify({
              glossaryTerms: response.generatedGlossaryTerms.map((term: any) => ({
                term: term.term,
                definition: term.definition,
                category: term.category,
                businessContext: term.businessContext,
                sourceTables: term.sourceTables,
                sourceColumns: term.sourceColumns
              }))
            }, null, 2),
            tokenCount: Math.floor(Math.random() * 300) + 500
          };
          setAiPrompts(prev => [...prev, glossaryPrompt]);
        }

        // Final completion will be handled by SignalR, just set results
        setResults(response);

        completed.push(`🎉 Auto-generation completed successfully!`);
        setRecentlyCompleted([...completed]);

        // Update counters with real data from API response (preserve final values)
        if (response.generatedTableContexts) {
          setTablesProcessed(response.generatedTableContexts.length);
          console.log('🔄 Final TablesProcessed set to:', response.generatedTableContexts.length);
        }
        if (response.generatedGlossaryTerms) {
          setGlossaryTermsGenerated(response.generatedGlossaryTerms.length);
          console.log('🔄 Final GlossaryTermsGenerated set to:', response.generatedGlossaryTerms.length);
        }
        if (response.relationshipAnalysis?.relationships) {
          setRelationshipsFound(response.relationshipAnalysis.relationships.length);
          console.log('🔄 Final RelationshipsFound set to:', response.relationshipAnalysis.relationships.length);
        }

        // Ensure final progress and completion state
        setGenerationProgress(100);
        setCurrentTask('Auto-generation completed successfully!');
        setCurrentStage('Completed');

        // Add final completion log entry
        addLogEntry('success', `🎉 Auto-generation completed successfully! Generated ${response.generatedTableContexts?.length || 0} table contexts and ${response.generatedGlossaryTerms?.length || 0} glossary terms.`, {
          tableContexts: response.generatedTableContexts?.length || 0,
          glossaryTerms: response.generatedGlossaryTerms?.length || 0,
          relationships: response.relationshipAnalysis?.relationships?.length || 0,
          processingTime: response.processingTime,
          success: response.success
        });

        if (response.success) {
          message.success(`Auto-generation completed! Generated ${response.generatedTableContexts?.length || 0} table contexts and ${response.generatedGlossaryTerms?.length || 0} glossary terms.`);
        } else {
          message.warning('Auto-generation completed with warnings. Please review the results.');
        }

        // Show log viewer automatically when completed
        setTimeout(() => {
          setShowLogViewer(true);
        }, 1000);
      } catch (err) {
        const errorMessage = err instanceof Error ? err.message : 'Auto-generation failed';
        addLogEntry('error', `❌ Auto-generation failed: ${errorMessage}`, {
          error: err instanceof Error ? err.stack : err,
          timestamp: new Date().toISOString()
        });
        setError(errorMessage);
        setCurrentTask('Auto-generation failed');
        setCurrentStage('Error');
        setCurrentTable('');
        setCurrentColumn('');
        message.error(errorMessage);
        console.error('Auto-generation error:', err);

        // Show log viewer on error as well
        setTimeout(() => {
          setShowLogViewer(true);
        }, 1000);
      }
    } finally {
      // Ensure we always clean up and stop the generation process
      // Only set isGenerating to false after a delay to allow final statistics to be displayed
      setTimeout(() => {
        setIsGenerating(false);
        console.log('🔄 Generation process completed, isGenerating set to false');
      }, 1000);

      setCurrentTable('');
      setCurrentColumn('');

      // Force completion if we somehow got stuck
      setTimeout(() => {
        if (generationProgress >= 95 && isGenerating) {
          console.warn('Force completing auto-generation process');
          setIsGenerating(false);
          setGenerationProgress(100);
          setCurrentTask('Auto-generation completed (forced)');
          setCurrentStage('Completed');
          setCurrentTable('');
          setCurrentColumn('');
        }
      }, 3000);
    }
  }, [generateTableContexts, generateGlossaryTerms, analyzeRelationships, overwriteExisting, confidenceThreshold, selectedTables, mockMode]);

  const handleApplyResults = useCallback(async (editedResults: any) => {
    if (!editedResults) return;

    try {
      setIsGenerating(true);
      setCurrentTask('Applying auto-generated context...');

      await tuningApi.applyAutoGeneratedContext(editedResults);

      message.success('Auto-generated context applied successfully!');
      setResults(null);

      if (onRefresh) {
        onRefresh();
      }
    } catch (err) {
      const errorMessage = err instanceof Error ? err.message : 'Failed to apply auto-generated context';
      message.error(errorMessage);
    } finally {
      setIsGenerating(false);
      setCurrentTask('');
    }
  }, [onRefresh]);

  const handleDiscardResults = useCallback(() => {
    setResults(null);
    setError(null);
    setGenerationProgress(0);
    setCurrentTask('');
  }, []);

  return (
    <div style={{ padding: '24px' }}>
      <Card>
        <div style={{ marginBottom: '24px' }}>
          <Title level={3}>
            <RobotOutlined style={{ marginRight: '8px', color: '#1890ff' }} />
            Auto-Generate Business Context
          </Title>
          <Paragraph type="secondary">
            Automatically analyze your database schema and generate intelligent business context,
            including table descriptions, business glossary terms, and relationship analysis.
            This feature uses AI to bootstrap your tuning process with smart defaults.
          </Paragraph>
        </div>

        {error && (
          <Alert
            message="Auto-Generation Error"
            description={typeof error === 'string' ? error : error?.message || 'An error occurred'}
            type="error"
            showIcon
            style={{ marginBottom: '24px' }}
            action={
              <Button size="small" onClick={() => setError(null)}>
                Dismiss
              </Button>
            }
          />
        )}

        {!results && !isGenerating && (
          <>
            {/* Table Selection Section */}
            <Card title="Table Selection" style={{ marginBottom: '24px' }}>
              <Row gutter={[16, 16]}>
                <Col span={24}>
                  <Space direction="vertical" style={{ width: '100%' }}>
                    <div style={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between' }}>
                      <Space>
                        <DatabaseOutlined style={{ color: '#1890ff' }} />
                        <Text strong>Select Tables to Process</Text>
                        <Tag color="blue">{selectedTables.length} of {availableTables.length} selected</Tag>
                      </Space>
                      <Switch
                        checked={selectAllTables}
                        onChange={handleSelectAllTablesChange}
                        checkedChildren="All"
                        unCheckedChildren="Custom"
                      />
                    </div>

                    <Text type="secondary" style={{ fontSize: '12px' }}>
                      Choose which database tables to include in the auto-generation process.
                      You can select all tables or choose specific ones for targeted analysis.
                    </Text>

                    {loadingTables ? (
                      <div style={{ textAlign: 'center', padding: '20px' }}>
                        <Spin size="large" />
                        <div style={{ marginTop: '8px' }}>
                          <Text type="secondary">Loading available tables...</Text>
                        </div>
                      </div>
                    ) : (
                      <Select
                        mode="multiple"
                        style={{ width: '100%' }}
                        placeholder="Select tables to process"
                        value={selectedTables}
                        onChange={handleTableSelectionChange}
                        showSearch
                        filterOption={(input, option) =>
                          (option?.label ?? '').toLowerCase().includes(input.toLowerCase())
                        }
                        options={availableTables.map(table => ({
                          label: table,
                          value: table,
                        }))}
                        maxTagCount="responsive"
                        tagRender={(props) => {
                          const { label, closable, onClose } = props;
                          return (
                            <Tag
                              color="blue"
                              closable={closable}
                              onClose={onClose}
                              style={{ marginRight: 3 }}
                            >
                              <TableOutlined style={{ marginRight: 4 }} />
                              {label}
                            </Tag>
                          );
                        }}
                      />
                    )}

                    {selectedTables.length > 0 && (
                      <div style={{ marginTop: '8px' }}>
                        <Text type="secondary" style={{ fontSize: '11px' }}>
                          <CheckCircleOutlined style={{ color: '#52c41a', marginRight: '4px' }} />
                          {selectedTables.length} table{selectedTables.length !== 1 ? 's' : ''} selected for processing
                        </Text>
                      </div>
                    )}
                  </Space>
                </Col>
              </Row>
            </Card>

            <Card title="Generation Options" style={{ marginBottom: '24px' }}>
              <Row gutter={[16, 16]}>
                <Col span={24}>
                  <Title level={5}>What to Generate</Title>
                </Col>

              <Col span={8}>
                <Card size="small" style={{ height: '100%' }}>
                  <Space direction="vertical" style={{ width: '100%' }}>
                    <div style={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between' }}>
                      <Space>
                        <TableOutlined style={{ color: '#52c41a' }} />
                        <Text strong>Table Contexts</Text>
                      </Space>
                      <Switch
                        checked={generateTableContexts}
                        onChange={setGenerateTableContexts}
                      />
                    </div>
                    <Text type="secondary" style={{ fontSize: '12px' }}>
                      Generate business-friendly descriptions, use cases, and context for each database table
                    </Text>
                  </Space>
                </Card>
              </Col>

              <Col span={8}>
                <Card size="small" style={{ height: '100%' }}>
                  <Space direction="vertical" style={{ width: '100%' }}>
                    <div style={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between' }}>
                      <Space>
                        <BookOutlined style={{ color: '#1890ff' }} />
                        <Text strong>Business Glossary</Text>
                      </Space>
                      <Switch
                        checked={generateGlossaryTerms}
                        onChange={setGenerateGlossaryTerms}
                      />
                    </div>
                    <Text type="secondary" style={{ fontSize: '12px' }}>
                      Create business glossary terms from column names and data patterns
                    </Text>
                  </Space>
                </Card>
              </Col>

              <Col span={8}>
                <Card size="small" style={{ height: '100%' }}>
                  <Space direction="vertical" style={{ width: '100%' }}>
                    <div style={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between' }}>
                      <Space>
                        <ShareAltOutlined style={{ color: '#722ed1' }} />
                        <Text strong>Relationships</Text>
                      </Space>
                      <Switch
                        checked={analyzeRelationships}
                        onChange={setAnalyzeRelationships}
                      />
                    </div>
                    <Text type="secondary" style={{ fontSize: '12px' }}>
                      Analyze table relationships and identify business domains
                    </Text>
                  </Space>
                </Card>
              </Col>

              <Col span={24}>
                <Divider />
                <Title level={5}>Advanced Options</Title>
              </Col>

              <Col span={12}>
                <Space direction="vertical" style={{ width: '100%' }}>
                  <Text strong>Confidence Threshold</Text>
                  <InputNumber
                    min={0.1}
                    max={1.0}
                    step={0.1}
                    value={confidenceThreshold}
                    onChange={(value) => setConfidenceThreshold(value || 0.6)}
                    style={{ width: '100%' }}
                    formatter={(value) => `${Math.round((value || 0) * 100)}%`}
                    parser={(value) => (parseFloat(value?.replace('%', '') || '60') / 100)}
                  />
                  <Text type="secondary" style={{ fontSize: '12px' }}>
                    Minimum confidence score for generated content (higher = more selective)
                  </Text>
                </Space>
              </Col>

              <Col span={8}>
                <Space direction="vertical" style={{ width: '100%' }}>
                  <div style={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between' }}>
                    <Text strong>Overwrite Existing</Text>
                    <Switch
                      checked={overwriteExisting}
                      onChange={setOverwriteExisting}
                    />
                  </div>
                  <Text type="secondary" style={{ fontSize: '12px' }}>
                    Replace existing business context with auto-generated content
                  </Text>
                </Space>
              </Col>

              <Col span={8}>
                <Space direction="vertical" style={{ width: '100%' }}>
                  <div style={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between' }}>
                    <Text strong style={mockMode ? { color: '#722ed1' } : {}}>
                      Mock Mode {mockMode && '🧪'}
                    </Text>
                    <Switch
                      checked={mockMode}
                      onChange={setMockMode}
                      checkedChildren="Mock"
                      unCheckedChildren="Real"
                    />
                  </div>
                  <Text type="secondary" style={{ fontSize: '12px' }}>
                    {mockMode
                      ? '🧪 Test mode: Process real tables/columns but skip AI API calls'
                      : 'Normal mode: Send actual AI API requests for business context'
                    }
                  </Text>
                </Space>
              </Col>
            </Row>

            <Divider />

            <div style={{ textAlign: 'center' }}>
              <Space>
                <Button
                  type="primary"
                  size="large"
                  icon={<PlayCircleOutlined />}
                  onClick={handleAutoGenerate}
                  disabled={
                    (!generateTableContexts && !generateGlossaryTerms && !analyzeRelationships) ||
                    selectedTables.length === 0 ||
                    loadingTables
                  }
                  style={mockMode ? { backgroundColor: '#722ed1', borderColor: '#722ed1' } : {}}
                >
                  {mockMode ? 'Start Mock Generation' : 'Start Auto-Generation'}
                  {selectedTables.length > 0 && ` (${selectedTables.length} table${selectedTables.length !== 1 ? 's' : ''})`}
                  {mockMode && ' - No AI Calls'}
                </Button>

                {/* Debug button to test SignalR */}
                <Button
                  size="small"
                  onClick={async () => {
                    if ((window as any).signalRConnection) {
                      try {
                        console.log('🧪 Testing SignalR connection...');
                        await (window as any).signalRConnection.invoke('GetConnectionInfo');
                        console.log('✅ SignalR test successful');
                      } catch (error) {
                        console.error('❌ SignalR test failed:', error);
                      }
                    } else {
                      console.warn('❌ No SignalR connection available');
                    }
                  }}
                >
                  Test SignalR
                </Button>
              </Space>
            </div>
          </Card>
          </>
        )}

        {(isGenerating || (results && generationProgress >= 100)) && (
          <AutoGenerationProgress
            progress={generationProgress}
            currentTask={currentTask}
            currentTable={currentTable}
            currentStage={currentStage}
            currentColumn={currentColumn}
            tablesProcessed={tablesProcessed || 0}
            totalTables={totalTables || 0}
            columnsProcessed={columnsProcessed || 0}
            totalColumns={totalColumns || 0}
            glossaryTermsGenerated={glossaryTermsGenerated || 0}
            relationshipsFound={relationshipsFound || 0}
            recentlyCompleted={recentlyCompleted}
            processingDetails={processingDetails}
            aiPrompts={aiPrompts}
            mockMode={mockMode}
            processingLog={processingLog}
            onShowLogViewer={() => setShowLogViewer(true)}
          />
        )}

        {results && !isGenerating && (
          <>
            {/* Completion notification with log viewer button */}
            {processingLog.length > 0 && (
              <Alert
                message="Auto-Generation Completed"
                description={
                  <div>
                    <p>The auto-generation process has completed successfully. You can review the detailed processing log to see all the steps that were performed.</p>
                    <Button
                      type="link"
                      icon={<FileTextOutlined />}
                      onClick={() => setShowLogViewer(true)}
                      style={{ padding: 0 }}
                    >
                      View Complete Processing Log ({processingLog.length} entries)
                    </Button>
                  </div>
                }
                type="success"
                showIcon
                style={{ marginBottom: '24px' }}
              />
            )}

            <AutoGenerationResults
              results={results}
              onApply={handleApplyResults}
              onDiscard={handleDiscardResults}
            />
          </>
        )}

        {/* Processing Log Viewer */}
        <ProcessingLogViewer
          visible={showLogViewer}
          onClose={() => setShowLogViewer(false)}
          logs={processingLog}
          title="Auto-Generation Processing Log"
        />
      </Card>
    </div>
  );
};

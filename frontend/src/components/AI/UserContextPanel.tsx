import React, { useState, useEffect } from 'react';
import {
  Box,
  Card,
  CardContent,
  Typography,
  Chip,
  List,
  ListItem,
  ListItemText,
  ListItemIcon,
  Grid,
  Accordion,
  AccordionSummary,
  AccordionDetails,
  CircularProgress,
  Alert,
  Divider,
  Avatar,
  Badge,
} from '@mui/material';
import {
  ExpandMore as ExpandMoreIcon,
  Person as PersonIcon,
  TableChart as TableChartIcon,
  FilterList as FilterListIcon,
  Pattern as PatternIcon,
  TrendingUp as TrendingUpIcon,
  Schedule as ScheduleIcon,
  Domain as DomainIcon,
  Insights as InsightsIcon,
} from '@mui/icons-material';
import { ApiService, UserContextResponse } from '../../services/api';

const UserContextPanel: React.FC = () => {
  const [userContext, setUserContext] = useState<UserContextResponse | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    loadUserContext();
  }, []);

  const loadUserContext = async () => {
    try {
      setLoading(true);
      const context = await ApiService.getUserContext();
      setUserContext(context);
    } catch (err: any) {
      setError(err.response?.data?.message || 'Failed to load user context');
    } finally {
      setLoading(false);
    }
  };

  const getDomainColor = (domain: string) => {
    switch (domain.toLowerCase()) {
      case 'sales': return 'success';
      case 'marketing': return 'primary';
      case 'finance': return 'warning';
      case 'hr': return 'info';
      case 'operations': return 'secondary';
      default: return 'default';
    }
  };

  const getIntentIcon = (intent: string) => {
    switch (intent.toLowerCase()) {
      case 'aggregation': return <TrendingUpIcon />;
      case 'trend': return <TrendingUpIcon />;
      case 'comparison': return <InsightsIcon />;
      case 'filtering': return <FilterListIcon />;
      default: return <PatternIcon />;
    }
  };

  const formatDate = (dateString: string) => {
    return new Date(dateString).toLocaleDateString('en-US', {
      year: 'numeric',
      month: 'short',
      day: 'numeric',
      hour: '2-digit',
      minute: '2-digit',
    });
  };

  if (loading) {
    return (
      <Box display="flex" justifyContent="center" alignItems="center" minHeight={200}>
        <CircularProgress />
      </Box>
    );
  }

  if (error) {
    return (
      <Alert severity="error" sx={{ mb: 2 }}>
        {error}
      </Alert>
    );
  }

  if (!userContext) {
    return (
      <Alert severity="info" sx={{ mb: 2 }}>
        No user context available. Start asking questions to build your AI profile!
      </Alert>
    );
  }

  return (
    <Box>
      <Typography variant="h5" gutterBottom>
        <PersonIcon sx={{ mr: 1, verticalAlign: 'middle' }} />
        Your AI Profile
      </Typography>
      <Typography variant="body2" color="text.secondary" gutterBottom>
        AI learns from your query patterns to provide better suggestions and insights
      </Typography>

      {/* Domain and Overview */}
      <Card sx={{ mb: 3 }}>
        <CardContent>
          <Grid container spacing={3}>
            <Grid size={{ xs: 12, md: 6 }}>
              <Box display="flex" alignItems="center" mb={2}>
                <Avatar sx={{ bgcolor: 'primary.main', mr: 2 }}>
                  <DomainIcon />
                </Avatar>
                <Box>
                  <Typography variant="h6">Domain Focus</Typography>
                  <Chip 
                    label={userContext.domain || 'General'}
                    color={getDomainColor(userContext.domain)}
                    size="small"
                  />
                </Box>
              </Box>
            </Grid>
            <Grid size={{ xs: 12, md: 6 }}>
              <Box display="flex" alignItems="center" mb={2}>
                <Avatar sx={{ bgcolor: 'secondary.main', mr: 2 }}>
                  <ScheduleIcon />
                </Avatar>
                <Box>
                  <Typography variant="h6">Last Updated</Typography>
                  <Typography variant="body2" color="text.secondary">
                    {formatDate(userContext.lastUpdated)}
                  </Typography>
                </Box>
              </Box>
            </Grid>
          </Grid>
        </CardContent>
      </Card>

      {/* Preferred Tables */}
      {userContext.preferredTables.length > 0 && (
        <Accordion sx={{ mb: 2 }}>
          <AccordionSummary expandIcon={<ExpandMoreIcon />}>
            <Box display="flex" alignItems="center">
              <TableChartIcon sx={{ mr: 1 }} />
              <Typography variant="h6">
                Preferred Tables
              </Typography>
              <Badge 
                badgeContent={userContext.preferredTables.length} 
                color="primary" 
                sx={{ ml: 2 }}
              >
                <Box />
              </Badge>
            </Box>
          </AccordionSummary>
          <AccordionDetails>
            <Box display="flex" flexWrap="wrap" gap={1}>
              {userContext.preferredTables.map((table, index) => (
                <Chip 
                  key={index}
                  label={table}
                  variant="outlined"
                  icon={<TableChartIcon />}
                  size="small"
                />
              ))}
            </Box>
          </AccordionDetails>
        </Accordion>
      )}

      {/* Common Filters */}
      {userContext.commonFilters.length > 0 && (
        <Accordion sx={{ mb: 2 }}>
          <AccordionSummary expandIcon={<ExpandMoreIcon />}>
            <Box display="flex" alignItems="center">
              <FilterListIcon sx={{ mr: 1 }} />
              <Typography variant="h6">
                Common Filters
              </Typography>
              <Badge 
                badgeContent={userContext.commonFilters.length} 
                color="secondary" 
                sx={{ ml: 2 }}
              >
                <Box />
              </Badge>
            </Box>
          </AccordionSummary>
          <AccordionDetails>
            <Box display="flex" flexWrap="wrap" gap={1}>
              {userContext.commonFilters.map((filter, index) => (
                <Chip 
                  key={index}
                  label={filter}
                  variant="outlined"
                  icon={<FilterListIcon />}
                  size="small"
                  color="secondary"
                />
              ))}
            </Box>
          </AccordionDetails>
        </Accordion>
      )}

      {/* Query Patterns */}
      {userContext.recentPatterns.length > 0 && (
        <Accordion>
          <AccordionSummary expandIcon={<ExpandMoreIcon />}>
            <Box display="flex" alignItems="center">
              <PatternIcon sx={{ mr: 1 }} />
              <Typography variant="h6">
                Query Patterns
              </Typography>
              <Badge 
                badgeContent={userContext.recentPatterns.length} 
                color="info" 
                sx={{ ml: 2 }}
              >
                <Box />
              </Badge>
            </Box>
          </AccordionSummary>
          <AccordionDetails>
            <List>
              {userContext.recentPatterns.map((pattern, index) => (
                <React.Fragment key={index}>
                  <ListItem>
                    <ListItemIcon>
                      {getIntentIcon(pattern.intent)}
                    </ListItemIcon>
                    <ListItemText
                      primary={
                        <Box display="flex" alignItems="center" justifyContent="space-between">
                          <Typography variant="body1" sx={{ flexGrow: 1 }}>
                            {pattern.pattern}
                          </Typography>
                          <Box display="flex" gap={1}>
                            <Chip 
                              label={`${pattern.frequency}x`}
                              size="small"
                              color="primary"
                            />
                            <Chip 
                              label={pattern.intent}
                              size="small"
                              variant="outlined"
                            />
                          </Box>
                        </Box>
                      }
                      secondary={
                        <Box mt={1}>
                          <Typography variant="caption" color="text.secondary">
                            Last used: {formatDate(pattern.lastUsed)}
                          </Typography>
                          {pattern.associatedTables.length > 0 && (
                            <Box mt={1} display="flex" flexWrap="wrap" gap={0.5}>
                              {pattern.associatedTables.map((table, tableIndex) => (
                                <Chip 
                                  key={tableIndex}
                                  label={table}
                                  size="small"
                                  variant="outlined"
                                  sx={{ fontSize: '0.7rem', height: 20 }}
                                />
                              ))}
                            </Box>
                          )}
                        </Box>
                      }
                    />
                  </ListItem>
                  {index < userContext.recentPatterns.length - 1 && <Divider />}
                </React.Fragment>
              ))}
            </List>
          </AccordionDetails>
        </Accordion>
      )}

      {/* Empty State */}
      {userContext.preferredTables.length === 0 && 
       userContext.commonFilters.length === 0 && 
       userContext.recentPatterns.length === 0 && (
        <Card>
          <CardContent sx={{ textAlign: 'center', py: 4 }}>
            <InsightsIcon sx={{ fontSize: 64, color: 'text.secondary', mb: 2 }} />
            <Typography variant="h6" gutterBottom>
              Start Building Your AI Profile
            </Typography>
            <Typography variant="body2" color="text.secondary">
              Ask questions and execute queries to help AI learn your preferences and provide better suggestions.
            </Typography>
          </CardContent>
        </Card>
      )}
    </Box>
  );
};

export default UserContextPanel;

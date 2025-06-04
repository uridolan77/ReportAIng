import React from 'react';
import MinimalistQueryInterface from '../components/QueryInterface/MinimalistQueryInterface';
import { QueryResponse } from '../types/query';

const MinimalistQueryPage: React.FC = () => {
  const handleQuery = async (query: string): Promise<QueryResponse> => {
    try {
      const response = await fetch('/api/query', {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify({ query }),
      });

      if (!response.ok) {
        throw new Error(`HTTP error! status: ${response.status}`);
      }

      const data = await response.json();
      return data;
    } catch (error) {
      console.error('Query API error:', error);
      throw error;
    }
  };

  return (
    <MinimalistQueryInterface 
      onQuery={handleQuery}
      style={{
        fontFamily: "'Inter', -apple-system, BlinkMacSystemFont, 'Segoe UI', sans-serif"
      }}
    />
  );
};

export default MinimalistQueryPage;

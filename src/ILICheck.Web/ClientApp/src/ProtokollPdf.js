import React from 'react';
import { Document, Page, View, Text } from '@react-pdf/renderer';

export const ProtokollPdf = (
  <Document>
    <Page size="A4">
    <View>
        <Text style={{margin: 12, fontSize:24, textAlign: "justify"}}>Protokoll Title</Text>
      </View>
      <View>
        <Text style={{margin: 12, fontSize:14, textAlign: "justify"}}>"... include log text "</Text>
      </View>
    </Page>
  </Document>
);

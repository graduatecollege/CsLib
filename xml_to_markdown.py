#!/usr/bin/env python3
"""
Converts C# XML documentation files to Markdown format.

Copied from: https://github.com/graduatecollege/xml-doc-test
"""

import xml.etree.ElementTree as ET
import re
import sys
from pathlib import Path


class CSharpDocConverter:
    """Converts C# XML documentation to Markdown."""
    
    # Patterns that indicate compiler-generated members to skip
    COMPILER_GENERATED_PATTERNS = ['<G>$', '+']
    
    def __init__(self):
        self.generic_param_map = {
            '``0': 'T',
            '``1': 'T2',
            '``2': 'T3',
            '``3': 'T4',
            '`0': 'T',
            '`1': 'T2', 
            '`2': 'T3',
            '`3': 'T4',
        }
        # Map of member names to their extended type (for extension methods)
        self.extension_methods = {}
    
    def format_type_name(self, type_name):
        """Format a C# type name to be more readable."""
        if not type_name:
            return type_name
        
        result = type_name
        
        # First, replace generic parameter placeholders with readable names
        for placeholder, readable in self.generic_param_map.items():
            result = result.replace(placeholder, readable)
        
        # Convert generic type syntax from {T} to <T> - handle nested generics
        # Keep replacing until no more braces
        max_iterations = 10
        for _ in range(max_iterations):
            old_result = result
            result = re.sub(r'\{([^{}]+)\}', r'<\1>', result)
            if old_result == result:
                break
        
        # Simplify common System types (do this after generic conversion)
        result = result.replace('System.String', 'string')
        result = result.replace('System.Int32', 'int')
        result = result.replace('System.Int64', 'long')
        result = result.replace('System.Boolean', 'bool')
        result = result.replace('System.Double', 'double')
        result = result.replace('System.Decimal', 'decimal')
        result = result.replace('System.Object', 'object')
        result = result.replace('System.Void', 'void')
        result = result.replace('System.Func', 'Func')
        result = result.replace('System.Action', 'Action')
        
        return result
    
    def parse_member_name(self, name_attr):
        """Parse a member name attribute from XML documentation."""
        if not name_attr:
            return None, None, None
        
        # Format: "T:Namespace.Type" or "M:Namespace.Type.Method(params)" etc.
        match = re.match(r'^([A-Z]):(.+)$', name_attr)
        if not match:
            return None, None, None
        
        member_type = match.group(1)
        full_name = match.group(2)
        
        return member_type, full_name, name_attr
    
    def count_parameters(self, name_attr):
        """Count the number of parameters in a method signature."""
        # Extract parameters part
        match = re.search(r'\(([^)]*)\)', name_attr)
        if not match:
            return 0
        params = match.group(1)
        if not params:
            return 0
        
        # Count commas to get parameter count
        # Need to handle nested generics (braces) properly
        depth = 0
        count = 1  # If there's any text, there's at least one param
        for char in params:
            if char == '{':
                depth += 1
            elif char == '}':
                depth -= 1
            elif char == ',' and depth == 0:
                count += 1
        return count
    
    def extract_first_parameter_type(self, name_attr):
        """Extract the type of the first parameter from a method signature."""
        # Extract parameters part
        params_match = re.search(r'\(([^)]+)\)', name_attr)
        if not params_match:
            return None
        params = params_match.group(1)
        
        # Need to handle nested braces to find the first comma at depth 0
        depth = 0
        first_comma_idx = -1
        for i, char in enumerate(params):
            if char == '{':
                depth += 1
            elif char == '}':
                depth -= 1
            elif char == ',' and depth == 0:
                first_comma_idx = i
                break
        
        if first_comma_idx == -1:
            # No comma at depth 0, so the entire params is the first parameter
            return params
        else:
            return params[:first_comma_idx]
    
    def format_method_signature(self, full_name, name_attr=None):
        """Format a method signature for display."""
        # Handle constructors: Convert #ctor to class name
        if '#ctor' in full_name:
            # Extract class name (last part before #ctor)
            parts = full_name.split('.')
            class_name_idx = -1
            for i, part in enumerate(parts):
                if '#ctor' in part:
                    class_name_idx = i - 1
                    break
            
            if class_name_idx >= 0:
                class_name = parts[class_name_idx]
                # Extract parameters if present
                param_match = re.search(r'#ctor\(([^)]*)\)', full_name)
                if param_match:
                    params = param_match.group(1)
                    formatted_params = self.format_type_name(params)
                    return f"{class_name}({formatted_params})"
                else:
                    return f"{class_name}()"
        
        # Handle regular methods
        # Extract method name and parameters
        method_match = re.match(r'^(.+?)(\(.*\))?$', full_name)
        if not method_match:
            return full_name
        
        base_name = method_match.group(1)
        params = method_match.group(2) or '()'
        
        # Get just the method name (last part)
        name_parts = base_name.split('.')
        method_name = name_parts[-1]
        
        # Remove generic arity indicator from method name (e.g., ``1)
        method_name = re.sub(r'``\d+', '', method_name)
        
        # Check if this is an extension method using the pre-built map
        if name_attr and name_attr in self.extension_methods:
            # For extension methods, return just the method name and remaining parameters
            # (The extended type will be added separately in format_member_header)
            
            # Extract the parameters content (without parens)
            params_content = params[1:-1] if params.startswith('(') and params.endswith(')') else params
            
            # Find the first comma at depth 0 to separate first param from rest
            depth = 0
            first_comma_idx = -1
            for i, char in enumerate(params_content):
                if char == '{':
                    depth += 1
                elif char == '}':
                    depth -= 1
                elif char == ',' and depth == 0:
                    first_comma_idx = i
                    break
            
            if first_comma_idx != -1:
                # There are more parameters after the first one
                remaining = params_content[first_comma_idx+1:]
                formatted_remaining = self.format_type_name(remaining)
                return f"{method_name}({formatted_remaining})"
            else:
                # No other parameters
                return f"{method_name}()"
        
        # Not an extension method, format normally
        formatted_params = self.format_type_name(params)
        return f"{method_name}{formatted_params}"
    
    def format_full_method_name(self, full_name):
        """Format the full method name with proper generic parameters."""
        # Split on parameters first to avoid replacing generics in parameters
        if '(' in full_name:
            before_params, params = full_name.split('(', 1)
            params = '(' + params
        else:
            before_params = full_name
            params = ''
        
        # Remove method-level generic arity indicators (``N)
        # These indicate how many generic parameters the method has, but we don't need this info
        before_params = re.sub(r'``\d+', '', before_params)
        
        # Remove type-level generic arity indicators (`N) from the method name
        before_params = re.sub(r'`\d+', '', before_params)
        
        # Format the parameters using format_type_name (which handles ``0, ``1, etc. and braces)
        if params:
            params = self.format_type_name(params)
        
        result = before_params + params
        
        return result
    
    def format_member_header(self, member_type, full_name, name_attr=None):
        """Format a member header for the markdown output."""
        if member_type == 'T':  # Type
            type_name = full_name.split('.')[-1]
            return f"## {self.format_type_name(type_name)}\n\n**Type**: `{self.format_type_name(full_name)}`"
        
        elif member_type == 'M':  # Method
            signature = self.format_method_signature(full_name, name_attr)
            formatted_full_name = self.format_full_method_name(full_name)
            
            # Check if this is an extension method
            if name_attr and name_attr in self.extension_methods:
                extended_type = self.extension_methods[name_attr]
                formatted_type = self.format_type_name(extended_type)
                return f"### {self.format_type_name(signature)}\n\n**Extends**: `{formatted_type}`\n\n**Method**: `{self.format_type_name(formatted_full_name)}`"
            else:
                return f"### {self.format_type_name(signature)}\n\n**Method**: `{self.format_type_name(formatted_full_name)}`"
        
        elif member_type == 'P':  # Property
            prop_name = full_name.split('.')[-1]
            return f"### {prop_name}\n\n**Property**: `{self.format_type_name(full_name)}`"
        
        elif member_type == 'F':  # Field
            field_name = full_name.split('.')[-1]
            return f"### {field_name}\n\n**Field**: `{self.format_type_name(full_name)}`"
        
        elif member_type == 'E':  # Event
            event_name = full_name.split('.')[-1]
            return f"### {event_name}\n\n**Event**: `{self.format_type_name(full_name)}`"
        
        else:
            return f"### {full_name}\n\n**{member_type}**: `{full_name}`"
    
    def format_cref_value(self, cref):
        """Format a cref attribute value for display."""
        if not cref:
            return ''
        
        # Remove the member type prefix (T:, M:, P:, etc.)
        if ':' in cref:
            cref = cref.split(':', 1)[1]
        
        # Handle type-level generic arity (`N) in cref values
        # Replace `1 with <T>, `2 with <T1, T2>, etc.
        def replace_type_arity(match):
            count = int(match.group(1))
            if count == 1:
                return '<T>'
            else:
                types = ', '.join([f'T{i}' for i in range(1, count + 1)])
                return f'<{types}>'
        
        cref = re.sub(r'`(\d+)', replace_type_arity, cref)
        
        # Now apply normal type name formatting (handles nested generics, system types, etc.)
        return self.format_type_name(cref)
    
    def extract_element_text(self, element):
        """Extract all text content from an XML element, including text around child elements."""
        if element is None:
            return ''
        
        # Get all text parts including text around child elements
        parts = []
        
        # Add the element's text (before any child elements)
        if element.text:
            parts.append(element.text)
        
        # Add text from child elements and their tails
        for child in element:
            # Handle special inline XML tags
            if child.tag == 'see':
                # Extract and format the cref attribute
                cref = child.get('cref', '')
                if cref:
                    formatted_cref = self.format_cref_value(cref)
                    parts.append(f'`{formatted_cref}`')
            elif child.tag == 'paramref':
                # Extract the name attribute
                param_name = child.get('name', '')
                if param_name:
                    parts.append(f'`{param_name}`')
            elif child.tag == 'c':
                # Code element - extract text and wrap in backticks
                code_text = self.extract_element_text(child)
                if code_text:
                    parts.append(f'`{code_text}`')
            else:
                # Recursively get text from other child elements
                child_text = self.extract_element_text(child)
                if child_text:
                    parts.append(child_text)
            
            # Add the tail text (text after the child element)
            if child.tail:
                parts.append(child.tail)
        
        # Join all parts and normalize whitespace
        full_text = ''.join(parts)
        # Replace multiple spaces with single space, but preserve newlines
        full_text = re.sub(r' +', ' ', full_text)
        
        return full_text
    
    def extract_documentation(self, member_elem, inheritdoc_lookup=None):
        """Extract documentation text from a member element."""
        docs = []
        
        # Check for inheritdoc first
        inheritdoc = member_elem.find('inheritdoc')
        if inheritdoc is not None and inheritdoc_lookup is not None:
            cref = inheritdoc.get('cref', '')
            if cref and cref in inheritdoc_lookup:
                # Use the referenced member's documentation
                return self.extract_documentation(inheritdoc_lookup[cref], inheritdoc_lookup)
        
        # Get summary
        summary = member_elem.find('summary')
        if summary is not None:
            text = self.extract_element_text(summary).strip()
            if text:
                docs.append(text)
        
        # Get parameters
        for param in member_elem.findall('param'):
            param_name = param.get('name', '')
            param_text = self.extract_element_text(param).strip()
            if param_text:
                docs.append(f"**{param_name}**: {param_text}")
        
        # Get returns
        returns = member_elem.find('returns')
        if returns is not None:
            text = self.extract_element_text(returns).strip()
            if text:
                docs.append(f"**Returns**: {text}")
        
        # Get remarks
        remarks = member_elem.find('remarks')
        if remarks is not None:
            text = self.extract_element_text(remarks).strip()
            if text:
                docs.append(f"**Remarks**: {text}")
        
        # Get exceptions
        for exception in member_elem.findall('exception'):
            exception_type = exception.get('cref', '')
            exception_text = self.extract_element_text(exception).strip()
            if exception_text:
                docs.append(f"**Throws {self.format_type_name(exception_type)}**: {exception_text}")
        
        return '\n\n'.join(docs) if docs else '*No documentation available.*'
    
    def convert_xml_to_markdown(self, xml_file):
        """Convert an XML documentation file to Markdown."""
        tree = ET.parse(xml_file)
        root = tree.getroot()
        
        # Get assembly name
        assembly = root.find('assembly/name')
        assembly_name = assembly.text if assembly is not None else 'Unknown'
        
        markdown = [f"# {assembly_name}\n"]
        markdown.append(f"*Generated from {Path(xml_file).name}*\n")
        
        # First pass: Build a lookup map for all members (including compiler-generated ones)
        # This is needed for inheritdoc resolution and extension method detection
        members_elem = root.find('members')
        inheritdoc_lookup = {}
        
        if members_elem is not None:
            for member in members_elem.findall('member'):
                name_attr = member.get('name')
                if name_attr:
                    inheritdoc_lookup[name_attr] = member
            
            # Build extension methods map by comparing parameter counts
            for member in members_elem.findall('member'):
                name_attr = member.get('name')
                if not name_attr:
                    continue
                
                # Check if this member has inheritdoc
                inheritdoc = member.find('inheritdoc')
                if inheritdoc is not None:
                    cref = inheritdoc.get('cref', '')
                    if cref and cref in inheritdoc_lookup:
                        # Compare parameter counts
                        inheritdoc_params = self.count_parameters(name_attr)
                        referenced_params = self.count_parameters(cref)
                        
                        # If the inheritdoc member has one more parameter, it's an extension method
                        if inheritdoc_params == referenced_params + 1:
                            # Extract the first parameter type (the extended type)
                            extended_type = self.extract_first_parameter_type(name_attr)
                            if extended_type:
                                self.extension_methods[name_attr] = extended_type
        
        # Second pass: Process members for output
        if members_elem is not None:
            for member in members_elem.findall('member'):
                name_attr = member.get('name')
                if not name_attr:
                    continue
                
                member_type, full_name, _ = self.parse_member_name(name_attr)
                if not member_type or not full_name:
                    continue
                
                # Skip compiler-generated members (they have strange names)
                if any(pattern in full_name for pattern in self.COMPILER_GENERATED_PATTERNS):
                    continue
                
                # Skip members not in the Grad namespace
                if not full_name.startswith('Grad.'):
                    continue
                
                # Skip property members (P:) as they're already included in the type member
                if member_type == 'P':
                    continue
                
                # Format the member header
                header = self.format_member_header(member_type, full_name, name_attr)
                markdown.append(f"\n{header}\n")
                
                # Extract and add documentation (with inheritdoc support)
                docs = self.extract_documentation(member, inheritdoc_lookup)
                markdown.append(docs)
                markdown.append("\n---\n")
        
        return '\n'.join(markdown)


def main():
    """Main entry point."""
    converter = CSharpDocConverter()
    
    # Find all XML files in the current directory
    xml_files = list(Path('.').glob('*/bin/Debug/net*/*.xml'))
    
    if not xml_files:
        print("No XML files found in the current directory.")
        return 1
    
    for xml_file in xml_files:
        print(f"Converting {xml_file}...")
        
        try:
            markdown = converter.convert_xml_to_markdown(xml_file)
            
            # Write to markdown file
            output_file = Path(Path(xml_file.with_suffix('.md').name))
            output_file.write_text(markdown)
            
            print(f"  -> Created {output_file}")
        
        except Exception as e:
            print(f"  Error: {e}")
            return 1
    
    print("\nConversion complete!")
    return 0


if __name__ == '__main__':
    sys.exit(main())
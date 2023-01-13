from multiprocessing.forkserver import connect_to_new_process
import xmltodict
import glob
import pprint
import re
from pathlib import Path

pp = pprint.PrettyPrinter(indent=4)

def get_param_text_from_xml(data, name):
    if isinstance(data['param'], list):
        for p in data['param']:
            if p['@name'] == name:
                return p['#text']
    else:
        return data['param']['#text']
    return '*missing*'

def gen_dts_function(data, isclass):
    outstr = f'\t\t/**\n'
    if 'summary' in data['xml']:
        summary = data['xml']['summary'].split('\n')[0]
        outstr += f'\t\t * {summary}\n'
    for p in data['parameters']:
        outstr += f'\t\t * @param {p["name"]} {get_param_text_from_xml(data["xml"], p["name"])}\n'
    if 'returns' in data['xml']:
        outstr += f'\t\t * @returns {data["xml"]["returns"]}\n'
    outstr += f'\t\t */\n'
    outstr += f'\t\t'
    if not isclass:
        outstr += 'function '
    outstr += f'{data["name"]}('
    for p in data['parameters']:
        outstr += f'{p["name"]}: {convert_type_to_js(p["type"])}'
        #if p['default'] is not None:
        #    outstr += f' = {p["default"]}'
        outstr += ', '
    if outstr.endswith(', '):
        outstr = outstr[:-2]
    if data['returntype'] is None:
        outstr += ');'
    else:
        outstr += f'): {convert_type_to_js(data["returntype"])};'
    return outstr

def gen_dts_property(data, isclass):
    outstr = f'\t\t/**\n'
    if 'summary' in data['xml']:
        summary = data['xml']['summary'].split('\n')[0]
        outstr += f'\t\t * {summary}\n'
    outstr += f'\t\t */\n'
    outstr += f'\t\t'
    if 'fakedtstype' in data['xml']:
        returntype = data['xml']['fakedtstype']
    else:
        returntype = data['returntype']
    if not isclass:
        outstr += 'let '
    outstr += f'{data["name"]}: {convert_type_to_js(returntype)};'
    return outstr

def gen_dts_enum(data):
    outstr = f'\t\t/**\n'
    if 'summary' in data['xml']:
        summary = data['xml']['summary'].split('\n')[0]
        outstr += f'\t\t * {summary}\n'
    outstr += f'\t\t */\n'
    outstr += f'\t\tenum {data["name"]} {{\n'
    for e in data['xml']['enum']:
        outstr += f'\t\t\t/**\n'
        outstr += f'\t\t\t * {e["#text"]}\n'
        outstr += f'\t\t\t */\n'
        outstr += f'\t\t\t{e["@name"]},\n'
    outstr += '\t\t}\n'
    return outstr

def convert_type_to_js(text):
    if '[]' in text:
        arr = '[]'
    else:
        arr = ''

    if text == 'double' or text == 'float' or 'int' in text:
        return 'number' + arr
    elif text == 'bool':
        return 'boolean' + arr
    elif text == 'object' or text == 'ExpandoObject':
        return 'any' + arr
    return text

def determine_text_type(text):
    #print('----------')
    #print(f'text: {text}')
    signature = text.replace('public ', '').replace('static ', '').replace('override', '').replace('Wrapper', '')
    #print(f'signature: {signature}')
    parameters = []
    if 'internal' in signature:
        return 'internal', None, None, None
    if 'private' in signature:
        return 'private', None, None, None
    if 'class ' in text or 'struct ' in text:
        return 'global', None, None, None
    if signature.strip().startswith('enum'):
        return 'enums', re.sub(r'[^\s]+\s+', r'', signature), None, None        
    if '(' not in text:
        returntype = signature.split(' ', 1)[0].strip()
        return 'properties', re.sub(r'[^\s]+\s+', r'', signature).rstrip(';'), None, returntype
    signaturefields = signature.split('(')
    if signaturefields[1] != ')':
        for sf in signaturefields[1].rstrip(')').split(','):
            #print(f'### {sf}')
            ptype, pname = sf.strip().split(' ', 1)
            if '=' in pname:
                defaultvalue = pname.split('=')[1].strip()
                pname = pname.split('=')[0].strip()
            else:
                defaultvalue = None
            parameters.append({ 'name': pname, 'type': ptype, 'default': defaultvalue })
    #print('parametertypes:')
    #for pt in parametertypes:
    #    print(f'\t{pt}')
    returntype = signaturefields[0].strip().split(' ')[0]
    name = re.sub(r'[^\s]+\s+', r'', signaturefields[0].strip())
    #print(f'name: {name}')
    #for p in parameters:
    #    print(f'pname: {p["name"]}, ptype: {p["type"]}')
    signature = re.sub(r'[^\s]+\s+', r'', signaturefields[0]) + '(' + re.sub(r'([^\s]+) ([^,]+)(,?\s*)', r'\2\3', signaturefields[1])
    #print(f'signature: {signature}')
    fields = text.split()
    if fields[0] == 'public' and ('Wrapper(' in fields[1] or 'QueryOptions(' in fields[1]):
        return 'constructors', name, parameters, returntype
    elif fields[1] == 'static':
        return 'staticmethods', name, parameters, returntype
    return 'methods', name, parameters, returntype

def get_sorted_comment_texts(texts):
    text = ''
    for  t in sorted(texts.keys()):
        text += texts[t]
    return text

def parse_attributes_line(line, attributes):
    mo = re.match(r'\[(.+?)\((.+?)\)\]', line)
    if mo is None:
        return
    attr_name = mo.group(1)
    attr_vals = mo.group(2)
    if attr_name not in attributes:
        attributes[attr_name] = {}
    for attr in attr_vals.split(','):
        mo = re.match(r'(.+)=(.+)', attr)
        if mo is None:
            return
        attributes[attr_name][mo.group(1).strip()] = mo.group(2).strip()


topics = {
    'GameConfiguration': { 'files': [ '../API/GameConfigurationWrapper.cs' ], 'asnamespace': True },
    'Angle2D': { 'files': [ '../API/Angle2DWrapper.cs' ], 'asnamespace': True },
    'BlockEntry' : { 'files': [ '../API/BlockEntryWrapper.cs' ] },
    'BlockMapQueryResult' : { 'files': [ '../API/BlockMapQueryResult.cs' ] },
    'BlockMap' : { 'files': [ '../API/BlockMapWrapper.cs' ] },
    'Data': { 'files': [ '../API/DataWrapper.cs' ], 'asnamespace': True },
    'ImageInfo': { 'files': [ '../API/ImageInfo.cs' ] },
    'Line2D': { 'files': [ '../API/Line2DWrapper.cs' ] },
    'Linedef': { 'files': [ '../API/LinedefWrapper.cs', '../API/MapElementWrapper.cs' ] },
    'Map': { 'files': [ '../API/MapWrapper.cs' ], 'asnamespace': True },
    'Plane': { 'files': [ '../API/PlaneWrapper.cs' ]},
    'Sector': { 'files': [ '../API/SectorWrapper.cs', '../API/MapElementWrapper.cs' ] },
    'Sidedef': { 'files': [ '../API/SidedefWrapper.cs', '../API/MapElementWrapper.cs' ] },
    'Thing': { 'files': [ '../API/ThingWrapper.cs', '../API/MapElementWrapper.cs' ] },
    'UDB': { 'files': [ '../API/UDBWrapper.cs' ] },
    'Vector2D': { 'files': [ '../API/Vector2DWrapper.cs' ] },
    'Vector3D': { 'files': [ '../API/Vector3DWrapper.cs' ] },
    'Vertex': { 'files': [ '../API/VertexWrapper.cs', '../API/MapElementWrapper.cs' ] },
    'VisualCamera': { 'files': [ '../API/VisualCameraWrapper.cs' ] },
    'QueryOptions': { 'files': [ '../QueryOptions.cs' ] },
}

dtsdata = {}

for topic in topics:
    dtsd = {
        'properties': [],
        'constructors': [],
        'methods': [],
        'staticmethods': [],
        'enums': []
    }
    texts = {
        'global': '',
        'properties': {},
        'constructors': {},
        'methods': {},
        'staticmethods': {},
        'enums': {}
    }
    memberattributes = {}
    for filename in topics[topic]['files']:
        topicname = filename.split('\\')[-1].replace('Wrapper.cs', '')

        with open(filename, 'r') as file:
            xmltext = ''
            parsingcomment = False
            incodeblock = False
            for line in file:
                line = line.strip()
                if line.startswith('['):
                    parse_attributes_line(line, memberattributes)
                elif line.startswith('///'):
                    parsingcomment = True
                    line = re.sub(r'^\s', r'', line.lstrip('/'))
                    if line.startswith('```'):
                        if incodeblock:
                            xmltext += '```\n'
                            incodeblock = False
                        else:
                            xmltext += '\n```js\n'
                            incodeblock = True
                    else:
                        xmltext += line + '\n'
                elif parsingcomment is True:
                    commenttext = ''
                    d = xmltodict.parse('<d>' + xmltext + '</d>')['d']
                    summary = d['summary']
                    texttype, signature, parameters, returntype = determine_text_type(line)
                    if texttype == 'global':
                        texts['global'] = f'{summary}\n'
                    elif texttype != 'internal' and texttype != 'private':
                        if texttype == 'properties':
                            dtsd['properties'].append({
                                'xml': d,
                                'name': signature,
                                'returntype': returntype
                            })
                        elif texttype == 'constructors':
                            dtsd['constructors'].append({
                                'xml': d,
                                'name': 'constructor',
                                'returntype': None,
                                'parameters': parameters
                            })
                        elif texttype == 'methods':
                            dtsd['methods'].append({
                                'xml': d,
                                'name': signature,
                                'returntype': returntype,
                                'parameters': parameters
                            })
                        elif texttype == 'staticmethods':
                            dtsd['staticmethods'].append({
                                'xml': d,
                                'name': signature,
                                'returntype': returntype,
                                'parameters': parameters
                            })
                        elif texttype == 'enums':
                            dtsd['enums'].append({
                                'xml': d,
                                'name': signature
                            })
                        commenttext += '\n---\n'
                        if 'version' in d:
                            commenttext += f'<span style="float:right;font-weight:normal;font-size:66%">Version: {d["version"]}</span>\n'
                        if 'UDBScriptSettings' in memberattributes:
                            commenttext += f'<span style="float:right;font-weight:normal;font-size:66%">Version: {memberattributes["UDBScriptSettings"]["MinVersion"]}</span>\n'
                        commenttext += f'### {signature}'
                        if parameters is not None:
                            commenttext += '('
                            for param in parameters:
                                commenttext += f'{param["name"]}: {param["type"]}, '
                            if commenttext.endswith(', '):
                                commenttext = commenttext[:-2]
                            commenttext += ')'
                        commenttext += '\n'
                        commenttext += f'{summary}\n'
                        if 'param' in d:
                            commenttext += '#### Parameters\n'
                            if isinstance(d['param'], list):
                                for p in d['param']:
                                    text = '*missing*'
                                    if '#text' in p:
                                        text = p['#text']
                                    commenttext += f'* {p["@name"]}: {text}\n'
                            else:
                                text ='*missing*'
                                if '#text' in d['param']:
                                    text = d['param']['#text'].replace('```', '\n```\n')
                                commenttext += f'* {d["param"]["@name"]}: {text}\n'
                        if 'enum' in d:
                            commenttext += '#### Options\n'
                            if isinstance(d['enum'], list):
                                for p in d['enum']:
                                    text = '*missing*'
                                    if '#text' in p:
                                        text = p['#text']
                                    commenttext += f'* {p["@name"]}: {text}\n'
                            else:
                                text ='*missing*'
                                if '#text' in d['enum']:
                                    text = d['enum']['#text'].replace('```', '\n```\n')
                                commenttext += f'* {d["enum"]["@name"]}: {text}\n'                                
                        if 'returns' in d:
                            commenttext += '#### Return value\n'
                            text = '*missing*'
                            if d['returns'] is not None:
                                text = d['returns']
                            commenttext += f'{text}\n'

                        if signature not in texts[texttype]:
                            texts[texttype][signature] = ''
                        texts[texttype][signature] += commenttext
                    xmltext = ''
                    memberattributes = {}
                    parsingcomment = False

        dtsdata[topic] = dtsd


    outfile = open(f'htmldoc/docs/{topic}.md', 'w')
    outfile.write(f'# {topic}\n\n')
    outfile.write(f'{texts["global"]}')
    if len(texts["constructors"]) > 0:
        outfile.write(f'## Constructors\n{get_sorted_comment_texts(texts["constructors"])}')
    if len(texts["staticmethods"]) > 0:
        outfile.write(f'## Static methods\n{get_sorted_comment_texts(texts["staticmethods"])}')
    if len(texts["properties"]) > 0:
        outfile.write(f'## Properties\n{get_sorted_comment_texts(texts["properties"])}')
    if len(texts["methods"]) > 0:
        outfile.write(f'## Methods\n{get_sorted_comment_texts(texts["methods"])}')
    if len(texts['enums']) > 0:
        outfile.write(f'## Enums\n{get_sorted_comment_texts(texts["enums"])}')
    outfile.close()


# Create the .d.ts file
dtsoutstr = 'declare namespace UDB {\n'
for key in dtsdata:
    if key == 'UDB':
        for m in dtsdata[key]['methods']:
            dtsoutstr += (gen_dts_function(m, False) + '\n')[1:]
    else:
        if 'asnamespace' in topics[key] and topics[key]['asnamespace'] is True:
            blocktype = 'namespace'
            isclass = False
        else:
            blocktype = 'class'
            isclass = True

        if len(dtsdata[key]['constructors']) > 0 or len(dtsdata[key]['methods']) > 0 or len(dtsdata[key]['properties']) > 0:
            dtsoutstr += f'\t{blocktype} {key} {{\n'
            # constructors
            for c in dtsdata[key]['constructors']:
                dtsoutstr += gen_dts_function(c, isclass) + '\n'
            # methods
            for m in dtsdata[key]['methods']:
                dtsoutstr += gen_dts_function(m, isclass) + '\n'
            # properties
            for p in dtsdata[key]['properties']:
                if not (p['name'] in topics and p['name'] == p['returntype']):
                    dtsoutstr += gen_dts_property(p, isclass) + '\n'
                else:
                    print(f'ignoring {p["name"]} in {key} - returntype {p["returntype"]}')
                    
            dtsoutstr += '\t}\n'

        # static methods and enums
        if len(dtsdata[key]['staticmethods']) > 0 or len(dtsdata[key]['enums']) > 0:
            dtsoutstr += f'\tnamespace {key} {{\n'
            if len(dtsdata[key]['staticmethods']) > 0:
                for m in dtsdata[key]['staticmethods']:
                    dtsoutstr += gen_dts_function(m, False) + '\n'
            if len(dtsdata[key]['enums']) > 0:
                for e in dtsdata[key]['enums']:
                    dtsoutstr += gen_dts_enum(e) + '\n'
            dtsoutstr += '\t}\n'
dtsoutstr += '}\n'

dtsfile = Path('../../../../Build/UDBScript/udbscript.d.ts')

if not dtsfile.parent.exists():
    dtsfile.mkdir(parents=True, exist_ok=True)

dtsfile.write_text(dtsoutstr)
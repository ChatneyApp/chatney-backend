// @ts-nocheck -- compiled by Cursor's Canvas runtime, not the backend TypeScript project.
import {
  Callout,
  Card,
  CardBody,
  CardHeader,
  Grid,
  H1,
  H2,
  Pill,
  Row,
  Stack,
  Stat,
  Table,
  Text,
  useCanvasState,
  useHostTheme,
} from "cursor/canvas";

type Perspective = "context" | "runtime" | "domain" | "authorization" | "assurance" | "tla-failures";

const perspectives: Array<{ id: Perspective; label: string }> = [
  { id: "context", label: "System context" },
  { id: "runtime", label: "Runtime flow" },
  { id: "domain", label: "Domain model" },
  { id: "authorization", label: "Authorization" },
  { id: "assurance", label: "Formal assurance" },
  { id: "tla-failures", label: "TLA+ counterexamples" },
];

function DiagramFrame({ children, caption }: { children: unknown; caption: string }) {
  const theme = useHostTheme();
  return (
    <div>
      <div
        style={{
          width: "100%",
          overflowX: "auto",
          border: `1px solid ${theme.stroke.secondary}`,
          borderRadius: 8,
          background: theme.bg.editor,
        }}
      >
        {children as any}
      </div>
      <Text size="small" tone="tertiary" style={{ marginTop: 8, fontSize: 14, lineHeight: "20px" }}>
        {caption}
      </Text>
    </div>
  );
}

function Arrow({
  x1,
  y1,
  x2,
  y2,
  label,
  dashed,
}: {
  x1: number;
  y1: number;
  x2: number;
  y2: number;
  label?: string;
  dashed?: boolean;
}) {
  const theme = useHostTheme();
  return (
    <g>
      <line
        x1={x1}
        y1={y1}
        x2={x2}
        y2={y2}
        stroke={theme.text.tertiary}
        strokeWidth="1.5"
        strokeDasharray={dashed ? "6 5" : undefined}
        markerEnd="url(#arrow)"
      />
      {label ? (
        <text
          x={(x1 + x2) / 2}
          y={(y1 + y2) / 2 - 8}
          textAnchor="middle"
          fill={theme.text.secondary}
          fontSize="14"
        >
          {label}
        </text>
      ) : null}
    </g>
  );
}

function Box({
  x,
  y,
  w,
  h,
  title,
  lines = [],
  accent,
  muted,
}: {
  x: number;
  y: number;
  w: number;
  h: number;
  title: string;
  lines?: string[];
  accent?: boolean;
  muted?: boolean;
}) {
  const theme = useHostTheme();
  return (
    <g>
      <rect
        x={x}
        y={y}
        width={w}
        height={h}
        rx="7"
        fill={accent ? theme.fill.secondary : muted ? theme.fill.quaternary : theme.bg.elevated}
        stroke={accent ? theme.accent.primary : theme.stroke.primary}
        strokeWidth={accent ? "2" : "1"}
      />
      <text x={x + 14} y={y + 25} fill={accent ? theme.accent.primary : theme.text.primary} fontSize="16" fontWeight="600">
        {title}
      </text>
      {lines.map((line, index) => (
        <text key={line} x={x + 14} y={y + 47 + index * 18} fill={theme.text.secondary} fontSize="14">
          {line}
        </text>
      ))}
    </g>
  );
}

function DiagramDefs() {
  const theme = useHostTheme();
  return (
    <defs>
      <marker id="arrow" viewBox="0 0 10 10" refX="9" refY="5" markerWidth="6" markerHeight="6" orient="auto-start-reverse">
        <path d="M 0 0 L 10 5 L 0 10 z" fill={theme.text.tertiary} />
      </marker>
    </defs>
  );
}

function ContextDiagram() {
  const theme = useHostTheme();
  return (
    <DiagramFrame caption="Deployment and integration view · derived from Program.cs and docker-compose.yaml">
      <svg viewBox="0 0 1000 520" style={{ display: "block", minWidth: 1000, width: "100%" }} role="img" aria-label="Chatney system context">
        <DiagramDefs />
        <text x="40" y="42" fill={theme.text.tertiary} fontSize="14">CLIENTS</text>
        <text x="332" y="42" fill={theme.text.tertiary} fontSize="14">APPLICATION</text>
        <text x="730" y="42" fill={theme.text.tertiary} fontSize="14">INFRASTRUCTURE</text>

        <Box x={40} y={78} w={205} h={100} title="Chat clients" lines={["GraphQL over HTTP", "WebSocket event stream"]} />
        <Box x={40} y={302} w={205} h={82} title="Operator" lines={["Install / uninstall", "GraphQL IDE in debug"]} muted />

        <Box x={330} y={70} w={310} h={325} title="ChatneyBackend · .NET 9" lines={["Single deployable monolith"]} accent />
        <Box x={358} y={132} w={254} h={70} title="Auth middleware" lines={["JWT → current user"]} />
        <Box x={358} y={220} w={254} h={70} title="HotChocolate GraphQL" lines={["Queries + mutations · /query"]} />
        <Box x={358} y={308} w={254} h={58} title="WebSocket connector" lines={["In-memory fan-out · /ws"]} />

        <Box x={744} y={65} w={215} h={92} title="PostgreSQL" lines={["RepoDb + Npgsql", "FluentMigrator schema"]} />
        <Box x={744} y={215} w={215} h={82} title="S3-compatible storage" lines={["Attachment blobs", "RustFS in local compose"]} />
        <Box x={744} y={357} w={215} h={78} title="Kafka" lines={["Provisioned only", "No runtime integration"]} muted />

        <Arrow x1={245} y1={128} x2={330} y2={128} label="HTTP / WS" />
        <Arrow x1={245} y1={343} x2={330} y2={343} label="bootstrap" />
        <Arrow x1={640} y1={120} x2={744} y2={112} label="SQL" />
        <Arrow x1={640} y1={260} x2={744} y2={256} label="objects" />
        <Arrow x1={640} y1={377} x2={744} y2={396} label="unused" dashed />
      </svg>
    </DiagramFrame>
  );
}

function RuntimeDiagram() {
  return (
    <DiagramFrame caption="Authenticated mutation path · WebSocket pushes occur after database writes">
      <svg viewBox="0 0 1000 520" style={{ display: "block", minWidth: 1000, width: "100%" }} role="img" aria-label="Runtime request flow">
        <DiagramDefs />
        <Box x={38} y={202} w={145} h={72} title="Client" lines={["Bearer JWT"]} />
        <Box x={230} y={72} w={190} h={82} title="Auth middleware" lines={["Validate token", "Load user"]} />
        <Box x={230} y={198} w={190} h={82} title="GraphQL resolver" lines={["Authorize attribute", "Domain mutation"]} accent />
        <Box x={230} y={332} w={190} h={82} title="Message hydrator" lines={["Batch related rows", "Shape response"]} />

        <Box x={492} y={72} w={190} h={82} title="RoleManager" lines={["Resolve scope", "Require permission"]} />
        <Box x={492} y={198} w={190} h={82} title="AppRepos / PgRepo" lines={["Generic CRUD", "13 repositories"]} />
        <Box x={492} y={332} w={190} h={82} title="WebSocket connector" lines={["Broadcast event", "Or target user"]} />

        <Box x={758} y={110} w={195} h={85} title="PostgreSQL" lines={["Primary state", "Transactions per repo call"]} />
        <Box x={758} y={285} w={195} h={85} title="Connected clients" lines={["Mutation event updates"]} />

        <Arrow x1={183} y1={236} x2={230} y2={113} label="request" />
        <Arrow x1={325} y1={154} x2={325} y2={198} label="principal" />
        <Arrow x1={420} y1={220} x2={492} y2={113} label="check" />
        <Arrow x1={420} y1={240} x2={492} y2={240} label="write" />
        <Arrow x1={682} y1={226} x2={758} y2={161} label="SQL" />
        <Arrow x1={587} y1={280} x2={587} y2={332} label="emit" />
        <Arrow x1={682} y1={373} x2={758} y2={328} label="push" />
        <Arrow x1={492} y1={260} x2={420} y2={373} label="hydrate" />
        <Arrow x1={230} y1={373} x2={183} y2={258} label="response" />
      </svg>
    </DiagramFrame>
  );
}

function DomainDiagram() {
  return (
    <DiagramFrame caption="Logical domain model · nullable scoped keys shown on UserRole · array-backed references are not foreign keys">
      <svg viewBox="0 0 1000 600" style={{ display: "block", minWidth: 1100, width: "100%" }} role="img" aria-label="Chatney domain data model">
        <DiagramDefs />
        <Box x={35} y={55} w={180} h={92} title="Role" lines={["id · permissions[]", "isProtected"]} />
        <Box x={300} y={40} w={205} h={110} title="User" lines={["id · roleId", "active / verified", "banned / muted"]} accent />
        <Box x={585} y={40} w={235} h={128} title="UserRole" lines={["userId + nullable scopes", "roleId", "allowlist[] − denylist[]"]} />

        <Box x={35} y={245} w={180} h={88} title="Workspace" lines={["contains channels"]} />
        <Box x={300} y={220} w={205} h={112} title="Channel" lines={["workspaceId", "channelTypeId", "logical refs; no DB FK"]} />
        <Box x={585} y={235} w={235} h={90} title="ChannelType" lines={["baseRoleId", "logical role reference"]} />
        <Box x={35} y={415} w={180} h={90} title="ChannelGroup" lines={["workspaceId", "channelIds[]"]} />

        <Box x={300} y={405} w={205} h={140} title="Message" lines={["userId · channelId", "parentId → thread", "replyTo → quoted message", "attachmentIds[] / previewIds[]"]} accent />
        <Box x={585} y={398} w={180} h={102} title="Reaction" lines={["messageId", "userId · code", "composite uniqueness"]} />
        <Box x={790} y={398} w={175} h={102} title="Draft" lines={["userId · channelId", "parentId", "logical uniqueness"]} />

        <Arrow x1={215} y1={101} x2={300} y2={95} label="global role" />
        <Arrow x1={505} y1={98} x2={585} y2={98} label="scoped overrides" />
        <Arrow x1={702} y1={168} x2={702} y2={235} label="scope" />
        <Arrow x1={215} y1={285} x2={300} y2={276} label="1 : many" />
        <Arrow x1={505} y1={276} x2={585} y2={280} label="typed by" />
        <Arrow x1={402} y1={332} x2={402} y2={405} label="contains" />
        <Arrow x1={215} y1={458} x2={300} y2={458} label="orders" dashed />
        <Arrow x1={505} y1={456} x2={585} y2={449} label="reactions" />
        <Arrow x1={505} y1={510} x2={790} y2={466} label="draft before send" dashed />
      </svg>
    </DiagramFrame>
  );
}

function AuthorizationDiagram() {
  return (
    <DiagramFrame caption="Effective permissions · first matching scope wins; row result is (role permissions ∪ allowlist) − denylist">
      <svg viewBox="0 0 1000 540" style={{ display: "block", minWidth: 1000, width: "100%" }} role="img" aria-label="Scoped authorization resolution">
        <DiagramDefs />
        <Box x={35} y={205} w={185} h={88} title="Actor + resource" lines={["userId", "workspace / type / channel"]} />
        <Box x={290} y={38} w={220} h={78} title="1 · Channel override?" lines={["Most specific scope"]} accent />
        <Box x={290} y={150} w={220} h={78} title="2 · Channel type?" lines={["Fallback scope"]} />
        <Box x={290} y={262} w={220} h={78} title="3 · Workspace?" lines={["Fallback scope"]} />
        <Box x={290} y={374} w={220} h={78} title="4 · Global role" lines={["users.roleId"]} />

        <Box x={610} y={125} w={330} h={110} title="Effective permission set" lines={["role.permissions ∪ userRole.allowlist", "then subtract userRole.denylist", "AllMighty bypasses checks"]} accent />
        <Box x={610} y={310} w={330} h={105} title="Mutation guard" lines={["Can / Require(permission)", "Scoped mutations use resource context", "Denied operation does not mutate state"]} />

        <Arrow x1={220} y1={249} x2={290} y2={77} label="resolve" />
        <Arrow x1={400} y1={116} x2={400} y2={150} label="none" />
        <Arrow x1={400} y1={228} x2={400} y2={262} label="none" />
        <Arrow x1={400} y1={340} x2={400} y2={374} label="none" />
        <Arrow x1={510} y1={77} x2={610} y2={165} label="match" />
        <Arrow x1={510} y1={189} x2={610} y2={175} label="match" />
        <Arrow x1={510} y1={301} x2={610} y2={185} label="match" />
        <Arrow x1={510} y1={413} x2={610} y2={195} label="fallback" />
        <Arrow x1={775} y1={235} x2={775} y2={310} label="require" />
      </svg>
    </DiagramFrame>
  );
}

function AssuranceDiagram() {
  const theme = useHostTheme();
  return (
    <DiagramFrame caption="Assurance perspective · TLA+ checks behavior and Alloy checks schema/transition structure; neither runs in production">
      <svg viewBox="0 0 1000 540" style={{ display: "block", minWidth: 1000, width: "100%" }} role="img" aria-label="Formal assurance model">
        <DiagramDefs />
        <text x="38" y="36" fill={theme.text.tertiary} fontSize="14">IMPLEMENTATION EVIDENCE</text>
        <Box x={38} y={62} w={220} h={82} title="C# domain mutations" lines={["GraphQL behavior"]} />
        <Box x={38} y={185} w={220} h={82} title="FluentMigrator" lines={["Authoritative DB schema"]} />
        <Box x={38} y={308} w={220} h={82} title="WebSocket events" lines={["Runtime notifications"]} />

        <text x="370" y="36" fill={theme.text.tertiary} fontSize="14">FORMAL MODELS</text>
        <Box x={370} y={78} w={250} h={122} title="TLA+ state machine" lines={["Mutation preconditions", "State transitions", "Permissions + event audits"]} accent />
        <Box x={370} y={272} w={250} h={110} title="Alloy relational model" lines={["Schema structure", "Cascade transitions", "Index catalog"]} />

        <text x="734" y="36" fill={theme.text.tertiary} fontSize="14">CHECKED PROPERTIES</text>
        <Box x={734} y={62} w={230} h={132} title="Core safety" lines={["Uniqueness", "Declared references", "Permission safety", "Event correspondence"]} accent />
        <Box x={734} y={255} w={230} h={145} title="Known gaps" lines={["Thread count exactness", "Attachment ownership", "Strong domain references", "Root draft DB uniqueness"]} />

        <Arrow x1={258} y1={103} x2={370} y2={124} label="mirrored" />
        <Arrow x1={258} y1={226} x2={370} y2={319} label="modeled" />
        <Arrow x1={258} y1={349} x2={370} y2={164} label="audited" />
        <Arrow x1={620} y1={139} x2={734} y2={128} label="model check" />
        <Arrow x1={620} y1={327} x2={734} y2={326} label="analysis" />
      </svg>
    </DiagramFrame>
  );
}

function TlaFailuresDiagram() {
  const theme = useHostTheme();
  return (
    <DiagramFrame caption="Three deterministic GapChecks traces · these stronger invariants are intentionally excluded from CoreSafety">
      <svg viewBox="0 0 1000 600" style={{ display: "block", minWidth: 1100, width: "100%" }} role="img" aria-label="TLA counterexample traces">
        <DiagramDefs />
        <text x="38" y="38" fill={theme.text.tertiary} fontSize="14">COUNTEREXAMPLE</text>
        <text x="310" y="38" fill={theme.text.tertiary} fontSize="14">MINIMAL TRACE</text>
        <text x="790" y="38" fill={theme.text.tertiary} fontSize="14">VIOLATION</text>

        <Box x={38} y={70} w={220} h={100} title="Thread count drift" lines={["ThreadCountsExact", "ThreadCountGap.cfg"]} accent />
        <Box x={310} y={60} w={190} h={62} title="1 · Add parent" lines={["childrenCount = 0"]} />
        <Box x={555} y={60} w={190} h={62} title="2 · Reject child" lines={["invalid replyTo", "parent already incremented"]} />
        <Box x={790} y={60} w={175} h={82} title="Count mismatch" lines={["count = 1", "actual children = 0"]} />
        <Arrow x1={500} y1={91} x2={555} y2={91} label="then" />
        <Arrow x1={745} y1={91} x2={790} y2={101} label="leaves" />

        <Box x={38} y={245} w={220} h={100} title="Attachment ownership" lines={["AttachmentOwnership", "AttachmentOwnershipGap.cfg"]} accent />
        <Box x={310} y={235} w={190} h={62} title="1 · User 2 uploads" lines={["attachment 5"]} />
        <Box x={555} y={235} w={190} h={62} title="2 · User 1 posts" lines={["attachmentIds = {5}"]} />
        <Box x={790} y={235} w={175} h={82} title="Cross-user reuse" lines={["message author ≠", "attachment owner"]} />
        <Arrow x1={500} y1={266} x2={555} y2={266} label="then" />
        <Arrow x1={745} y1={266} x2={790} y2={276} label="allows" />

        <Box x={38} y={420} w={220} h={100} title="Strong references" lines={["StrongDomainReferences", "StrongReferencesGap.cfg"]} accent />
        <Box x={310} y={410} w={190} h={62} title="1 · Install" lines={["channel types 1 and 2"]} />
        <Box x={555} y={410} w={190} h={62} title="2 · Add channel" lines={["channelTypeId = 8"]} />
        <Box x={790} y={410} w={175} h={82} title="Orphan channel" lines={["type 8 absent", "mutation succeeds"]} />
        <Arrow x1={500} y1={441} x2={555} y2={441} label="then" />
        <Arrow x1={745} y1={441} x2={790} y2={451} label="creates" />
      </svg>
    </DiagramFrame>
  );
}

const perspectiveNotes: Record<Perspective, { title: string; points: string[] }> = {
  context: {
    title: "Boundary",
    points: [
      "GraphQL is the only implemented HTTP API despite controller registration.",
      "Kafka exists in local infrastructure but has no code integration.",
      "The WebSocket connector is process-local, so horizontal scaling needs shared fan-out.",
    ],
  },
  runtime: {
    title: "Critical path",
    points: [
      "JWT middleware establishes identity; resolver attributes and RoleManager enforce access.",
      "Domain resolvers call repositories directly rather than a separate application-service layer.",
      "Messages are hydrated from related rows before returning to clients.",
    ],
  },
  domain: {
    title: "Modeling choices",
    points: [
      "UserRole is a scoped permission override keyed by user plus nullable resource scopes.",
      "Message parentId models thread membership; replyTo models a quote reference.",
      "Several relationships use integer arrays or logical references rather than database foreign keys.",
    ],
  },
  authorization: {
    title: "Resolution rule",
    points: [
      "Precedence is channel → channel type → workspace → global role.",
      "The C# runtime selects the first row at a matching scope; TLA+ can model multiple candidates.",
      "Allowlist adds permissions and denylist removes them after role expansion.",
    ],
  },
  assurance: {
    title: "Confidence boundary",
    points: [
      "Core invariants cover declared references, uniqueness, permissions, events, and install idempotence.",
      "Formal files are verification artifacts and do not enforce production behavior by themselves.",
      "Known gaps are intentionally represented rather than hidden by the core safety predicate.",
    ],
  },
  "tla-failures": {
    title: "What failed",
    points: [
      "GapChecks is designed to produce three invariant violations; no committed trace shows CoreSafety failing.",
      "The thread counter mutates before all message references are validated, so a rejected child can leave drift.",
      "Message creation does not enforce attachment ownership, and channel creation does not validate channelTypeId.",
    ],
  },
};

function PerspectiveDiagram({ perspective }: { perspective: Perspective }) {
  if (perspective === "context") return <ContextDiagram />;
  if (perspective === "runtime") return <RuntimeDiagram />;
  if (perspective === "domain") return <DomainDiagram />;
  if (perspective === "authorization") return <AuthorizationDiagram />;
  if (perspective === "assurance") return <AssuranceDiagram />;
  return <TlaFailuresDiagram />;
}

export default function ChatneyArchitecturePerspectives() {
  const [perspective, setPerspective] = useCanvasState<Perspective>("active-perspective", "context");
  const note = perspectiveNotes[perspective];

  return (
    <Stack gap={24} style={{ padding: 32, maxWidth: 1600, margin: "0 auto" }}>
      <div>
        <H1>Chatney architecture perspectives</H1>
        <Text tone="secondary" style={{ marginTop: 6, maxWidth: 920, fontSize: 16, lineHeight: "24px" }}>
          Six complementary views of the backend: deployment boundary, request flow, domain relationships,
          scoped authorization, formal assurance, and TLA+ counterexamples.
        </Text>
      </div>

      <Row gap={8} wrap>
        {perspectives.map((item) => (
          <Pill
            key={item.id}
            active={perspective === item.id}
            tone={perspective === item.id ? "info" : "neutral"}
            onClick={() => setPerspective(item.id)}
          >
            {item.label}
          </Pill>
        ))}
      </Row>

      <Grid columns="minmax(0, 4fr) minmax(260px, 1fr)" gap={20} align="start">
        <Card size="lg">
          <CardHeader trailing={<Pill size="sm" tone="info" active>Perspective</Pill>}>
            {perspectives.find((item) => item.id === perspective)?.label}
          </CardHeader>
          <CardBody style={{ padding: 18 }}>
            <PerspectiveDiagram perspective={perspective} />
          </CardBody>
        </Card>

        <Stack gap={14}>
          <H2>{note.title}</H2>
          {note.points.map((point, index) => (
            <Row key={point} gap={10} align="start">
              <Text tone="tertiary" weight="semibold" style={{ minWidth: 18 }}>{index + 1}</Text>
              <Text tone="secondary" style={{ fontSize: 15, lineHeight: "22px" }}>{point}</Text>
            </Row>
          ))}
        </Stack>
      </Grid>

      <Grid columns={4} gap={16}>
        <Stat value="1" label="Runtime deployable" />
        <Stat value="13" label="Repository-backed aggregates" />
        <Stat value="4" label="Authorization scopes" />
        <Stat value="2" label="Formal notations" />
      </Grid>

      <Callout tone="warning" title="Architecture risks visible across perspectives">
        WebSocket identity is accepted from a userId query parameter, scoped-role row uniqueness is not enforced
        by a database primary key, and several domain references are logical rather than constrained by foreign keys.
      </Callout>

      {perspective === "tla-failures" ? (
        <div>
          <H2>Counterexample findings</H2>
          <Text tone="secondary" style={{ marginBottom: 10 }}>
            The gap configurations deliberately falsify stronger properties. CoreSafety.cfg is expected to pass.
          </Text>
          <Table
            headers={["Invariant", "Counterexample trigger", "Architecture impact"]}
            rows={[
              ["ThreadCountsExact", "Rejected child with missing replyTo increments parent first", "Thread counts can disagree with stored rows"],
              ["AttachmentOwnership", "User 1 references an attachment uploaded by user 2", "IDOR-style cross-user blob reuse"],
              ["StrongDomainReferences", "Channel is created with absent channelTypeId", "Orphan domain rows can satisfy declared integrity"],
            ]}
            rowTone={["warning", "danger", "warning"]}
            striped
          />
        </div>
      ) : null}

      <div>
        <H2>Source-of-truth map</H2>
        <Text tone="secondary" style={{ marginBottom: 10 }}>
          Use migrations and runtime models for the current database shape; db.dbml appears to lag the scoped-role redesign.
        </Text>
        <Table
          headers={["Concern", "Primary evidence", "Interpretation"]}
          rows={[
            ["Runtime composition", "ChatneyBackend/Program.cs", ".NET services, GraphQL, repositories, S3"],
            ["Current schema", "ChatneyBackend/Infra/Migrations/", "Authoritative over db.dbml"],
            ["Domain behavior", "ChatneyBackend/Domains/", "Queries, mutations, models, permission checks"],
            ["Behavioral safety", "formal/tla/", "State transitions, invariants, expected gaps"],
            ["Relational safety", "formal/alloy/", "Schema, indexes, cascades, transition checks"],
          ]}
          striped
        />
      </div>
    </Stack>
  );
}
